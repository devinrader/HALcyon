using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HALcyon.Tests
{
    [TestClass]
    public class ConverterTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var customer = new Customer() { Id=1234, Name="John Doe" };

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new JsonResourceConverter());

            var result = JsonConvert.SerializeObject(customer, settings);

            dynamic test = JsonConvert.DeserializeObject<dynamic>(result);

            Assert.AreEqual("John Doe", test.name.Value);
            Assert.IsNotNull(test._links);
            Assert.IsNotNull(test._links.self);
            Assert.AreEqual("/customers/1234", test._links.self.href.Value);
            Assert.AreEqual(null, test.orders.Value);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var customer = new Customer() { Id = 1234, Name = null };

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new JsonResourceConverter());

            var result = JsonConvert.SerializeObject(customer, settings);

            dynamic test = JsonConvert.DeserializeObject<dynamic>(result);

            Assert.AreEqual(null, test.name.Value);
            Assert.IsNotNull(test._links);
            Assert.IsNotNull(test._links.self);
            Assert.AreEqual("/customers/1234", test._links.self.href.Value);
            Assert.AreEqual(null, test.orders.Value);

        }

        [TestMethod]
        public void TestMethod3()
        {
            var customer = new Customer()
            {
                Id = 1234,
                Name = "John Doe",
                Orders = new System.Collections.Generic.List<Order>() { 
                    new Order() { Id = 45, Price=1.45 },
                    new Order() { Id = 46, Price=3.45 },
                    new Order() { Id = 47, Price=2.58 }
                }
            };

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new JsonResourceConverter());

            var result = JsonConvert.SerializeObject(customer, settings);

            dynamic test = JsonConvert.DeserializeObject<dynamic>(result);

            Assert.AreEqual("John Doe", test.name.Value);
            Assert.IsNotNull(test._links);
            Assert.IsNotNull(test._links.self);
            Assert.AreEqual("/customers/1234", test._links.self.href.Value);
            Assert.AreEqual(3, test._links.orders.Count);
        }
    }
}
