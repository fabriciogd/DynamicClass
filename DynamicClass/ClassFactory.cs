namespace DynamicClass
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;

    /// <summary>
    /// A factory to create dynamic classes
    /// </summary>
    public class ClassFactory
    {
        private static volatile ClassFactory instance;
        private static object syncRoot = new Object();

        public static ClassFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ClassFactory();
                    }
                }

                return instance;
            }
        }

        private readonly ModuleBuilder module;
        private ReaderWriterLock rwLock;
        private Dictionary<Signature, Type> classes;

        private ClassFactory()
        {
            module = CreateModule();

            classes = new Dictionary<Signature, Type>();
            rwLock = new ReaderWriterLock();
        }

        private ModuleBuilder CreateModule()
        {
            AssemblyBuilder dynamicAssembly =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName("DynamicClass.Utility.DynamicClasses, Version=1.0.0.0"),
                    AssemblyBuilderAccess.Run,
                    new CustomAttributeBuilder[0]
                );

            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("DynamicClass.Utility.DynamicClasses.dll");
            return dynamicModule;
        }

        /// <summary>
        /// This method creates a new class with a given set of public properties and returns the System.Type object for the newly created class.
        /// <param name="properties">A list of <see cref="DynamicProperty"/></param> 
        /// <returns>Type</returns>
        /// </summary>
        public Type Create(IEnumerable<DynamicProperty> properties)
        {
            rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                Signature signature = new Signature(properties);
                Type type;
                if (!classes.TryGetValue(signature, out type))
                {
                    LockCookie cookie = rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        if (!classes.TryGetValue(signature, out type))
                        {
                            type = this.GenerateClass(signature.properties);
                            classes.Add(signature, type);
                        }
                    }
                    finally
                    {
                        rwLock.DowngradeFromWriterLock(ref cookie);
                    }
                }
                return type;
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }
        }

        private Type GenerateClass(DynamicProperty[] properties)
        {
            string typeName = "DynamicClass" + (classes.Count + 1);

            TypeBuilder tb = this.module.DefineType(typeName, TypeAttributes.Class |
                   TypeAttributes.Public, typeof(DynamicClass));

            FieldInfo[] fields = GenerateProperties(tb, properties);

            this.GenerateMethods(tb, fields);

            return tb.CreateType();
        }

        public FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties)
        {
            FieldInfo[] fields = new FieldBuilder[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                DynamicProperty dp = properties[i];

                // Field
                FieldBuilder fb = tb.DefineField($"<{properties[i].Name}>i__Field", dp.Type, FieldAttributes.Private);

                // Property
                PropertyBuilder pb = tb.DefineProperty(dp.Name, PropertyAttributes.HasDefault, dp.Type, null);

                // Get()
                MethodBuilder getter = tb.DefineMethod($"get_{dp.Name}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, CallingConventions.HasThis, dp.Type, Type.EmptyTypes);

                ILGenerator genGetter = getter.GetILGenerator();
                genGetter.Emit(OpCodes.Ldarg_0);
                genGetter.Emit(OpCodes.Ldfld, fb);
                genGetter.Emit(OpCodes.Ret);

                pb.SetGetMethod(getter);

                // Set()
                MethodBuilder mbSet = tb.DefineMethod($"set_{dp.Name}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, CallingConventions.HasThis, null, new Type[] { dp.Type });

                mbSet.DefineParameter(1, ParameterAttributes.In, dp.Name);

                ILGenerator genSet = mbSet.GetILGenerator();
                genSet.Emit(OpCodes.Ldarg_0);
                genSet.Emit(OpCodes.Ldarg_1);
                genSet.Emit(OpCodes.Stfld, fb);
                genSet.Emit(OpCodes.Ret);

                pb.SetSetMethod(mbSet);

                fields[i] = fb;
            }

            return fields;
        }

        private void GenerateMethods(TypeBuilder tb, FieldInfo[] fields)
        {
            // Equals()
            MethodBuilder equals = tb.DefineMethod("Equals", MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig, CallingConventions.HasThis, typeof(bool), new Type[] { typeof(object) });

            ILGenerator genEquals = equals.GetILGenerator();
            genEquals.DeclareLocal(tb.AsType());

            genEquals.Emit(OpCodes.Ldarg_1);
            genEquals.Emit(OpCodes.Isinst, tb.AsType());
            genEquals.Emit(OpCodes.Stloc_0);
            genEquals.Emit(OpCodes.Ldloc_0);

            Label equalsLabel = genEquals.DefineLabel();

            // GetHashCode();
            MethodBuilder getHashCode = tb.DefineMethod("GetHashCode", MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig, CallingConventions.HasThis, typeof(int), Type.EmptyTypes);

            ILGenerator genGetHashCode = getHashCode.GetILGenerator();
            genGetHashCode.DeclareLocal(typeof(int));

            int initHash = 0;

            for (int i = 0; i < fields.Length; i++)
                initHash = unchecked(initHash * (-1521134295) + fields[i].Name.GetHashCode());

            genGetHashCode.Emit(OpCodes.Ldc_I4, initHash);

            for (int i = 0; i < fields.Length; i++)
            {
                Type ft = fields[i].FieldType;

                Type ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                MethodInfo defaultEqualityComparer = ct.GetMethod("get_Default");

                // Equals()
                genEquals.Emit(OpCodes.Brfalse, equalsLabel);
                genEquals.Emit(OpCodes.Call, defaultEqualityComparer);
                genEquals.Emit(OpCodes.Ldarg_0);
                genEquals.Emit(OpCodes.Ldfld, fields[i]);
                genEquals.Emit(OpCodes.Ldloc_0);
                genEquals.Emit(OpCodes.Ldfld, fields[i]);
                genEquals.Emit(OpCodes.Callvirt, ct.GetMethod("Equals", new Type[] { ft, ft }));

                // GetHashCode();
                genGetHashCode.Emit(OpCodes.Stloc_0);
                genGetHashCode.Emit(OpCodes.Ldc_I4, -1521134295);
                genGetHashCode.Emit(OpCodes.Ldloc_0);
                genGetHashCode.Emit(OpCodes.Mul);
                genGetHashCode.Emit(OpCodes.Call, defaultEqualityComparer);
                genGetHashCode.Emit(OpCodes.Ldarg_0);
                genGetHashCode.Emit(OpCodes.Ldfld, fields[i]);
                genGetHashCode.Emit(OpCodes.Callvirt, ct.GetMethod("GetHashCode", new Type[] { ft }));
                genGetHashCode.Emit(OpCodes.Add);
            }

            genEquals.Emit(OpCodes.Ret);
            genEquals.MarkLabel(equalsLabel);
            genEquals.Emit(OpCodes.Ldc_I4_0);

            genGetHashCode.Emit(OpCodes.Stloc_0);
            genGetHashCode.Emit(OpCodes.Ldloc_0);
            genGetHashCode.Emit(OpCodes.Ret);
        }
    }
}
