﻿namespace DynamicClass.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class DynamicClass
    {
        [TestMethod]
        public void Dynamic_properties()
        {
            var properties = new List<DynamicProperty>()
            {
                new DynamicProperty("Id", typeof(System.Int32)),
                new DynamicProperty("Nome", typeof(System.String))
            };

            Type instance = ClassFactory.Instance.Create(properties);

            Assert.AreEqual(2, instance.GetProperties().Count());
        }

        [TestMethod]
        public void Dynamic_properties_hashcode()
        {
            var properties = new List<DynamicProperty>()
            {
                new DynamicProperty("Id", typeof(System.Int32)),
                new DynamicProperty("Nome", typeof(System.String))
            };

            var t = ClassFactory.Instance.Create(properties);

            var instance = (dynamic)Activator.CreateInstance(t);

            Assert.AreNotEqual(0, instance.GetHashCode());
        }

        [TestMethod]
        public void Dynamic_properties_equal()
        {
            var properties = new List<DynamicProperty>()
            {
                new DynamicProperty("Id", typeof(System.Int32))
            };

            var t1 = ClassFactory.Instance.Create(properties);
            var t2 = ClassFactory.Instance.Create(properties);

            var instance1 = (dynamic)Activator.CreateInstance(t1);
            var instance2 = (dynamic)Activator.CreateInstance(t1);

            instance1.SetDynamicProperty("Id", 1);
            instance2.SetDynamicProperty("Id", 1);

            var result = instance1.Equals(instance2);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Dynamic_properties_not_equal()
        {
            var properties = new List<DynamicProperty>()
            {
                new DynamicProperty("Id", typeof(System.Int32))
            };

            var t1 = ClassFactory.Instance.Create(properties);
            var t2 = ClassFactory.Instance.Create(properties);

            var instance1 = (dynamic)Activator.CreateInstance(t1);
            var instance2 = (dynamic)Activator.CreateInstance(t1);

            instance1.SetDynamicProperty("Id", 1);
            instance2.SetDynamicProperty("Id", 2);

            var result = instance1.Equals(instance2);

            Assert.IsFalse(result);
        }
    }
}
