using System;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using vehicles_api.Controllers;
using vehicles_api.Models;
using vehicles_api.Filters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace vehicles_api.tests
{
    public class TestVehiclesController
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private List<Vehicle> _goodVehicles = null;
        private List<Vehicle> _badVehicles = null;

        protected List<Vehicle> GoodVehicles
        {
            get
            {
                if (_goodVehicles == null)
                { _goodVehicles = GetGoodVehicles(); }
                return _goodVehicles;
            }
        }

        protected List<Vehicle> BadVehicles
        {
            get
            {
                if (_badVehicles == null)
                { _badVehicles = GetBadVehicles(); }
                return _badVehicles;
            }
        }

        private List<Vehicle> GetGoodVehicles()
        {
            var ans = new List<Vehicle>(new Vehicle[] {
                new Vehicle{ Year = 2006, Make = "Chevy", Model = "Sonic" },
                new Vehicle{ Year = 2006, Make = "Ford", Model = "Fiesta"},
                new Vehicle{ Year = 2008, Make = "Chevy", Model = "Cruze"},
                new Vehicle{ Year = 2009, Make = "Nissan", Model = "Sentra"},
                new Vehicle{ Year = 2011, Make = "Ford", Model = "Fusion"},
                new Vehicle{ Year = 2014, Make = "Subaru", Model = "Legacy"},
                new Vehicle{ Year = 2016, Make = "Chevy", Model = "Impala"}
            });
            return ans;
        }

        private List<Vehicle> GetBadVehicles()
        {
            var ans = new List<Vehicle>(new Vehicle[] {
                new Vehicle{ Year = 1905, Make = "Ford", Model = "Model F" }, // Bad year
                new Vehicle{ Year = 1905, Make = "", Model = "NW F"}, // Bad year and no Make
                new Vehicle{ Year = 1905, Make = "Ford", Model = ""}, // bad year and not Model
                new Vehicle{ Year = 1950, Make = "", Model = "Henry J"}, // no Make
                new Vehicle{ Year = 1965, Make = "Ford", Model = ""}, // no Model
                new Vehicle{ Year = 2011, Make = "", Model = ""}, // no Make or Model
                new Vehicle{ Year = 2051, Make = "Chevy", Model = "Impala"} // year too high, though I don't think this is unthinkable :)
            });
            return ans;
        }

        public TestVehiclesController()
        {
            // Setup test servers so we can fully vet the web api
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public async void AddGoodVehicles(int index)
        {
            // verify index against available test data
            Assert.InRange(index, 0, GoodVehicles.Count - 1);

            // Get requested test data and add it using the controller
            var input = GoodVehicles[index];
            var response = await _client.PostAsync("/vehicles", ToByteArrayContent(input));
            response.EnsureSuccessStatusCode();

            Assert.InRange((int)response.StatusCode, 200, 299); // Make sure the status code is success
            var respString = await response.Content.ReadAsStringAsync();
            var val = (Vehicle)JsonConvert.DeserializeObject<Vehicle>(respString);

            // Did it add what we told it to?
            Assert.Equal(input, val, new VehicleComparer());
            // Just check that the ID generated was witin an expected range
            Assert.InRange(val.Id, 1, Int32.MaxValue);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public async void AddBadVehicles(int index)
        {
            // verify index against available test data
            Assert.InRange(index, 0, BadVehicles.Count - 1);

            // Get requested test data and add it using the controller
            var input = BadVehicles[index];
            var response = await _client.PostAsync("/vehicles", ToByteArrayContent(input));

            Assert.InRange((int)response.StatusCode, 400, 499); // Make sure the status code is failure
        }

        [Fact]
        public async void TestGetVehicles()
        {
            HttpResponseMessage response = null;
            foreach (Vehicle v in GoodVehicles)
            {
                response = await _client.PostAsync("/vehicles", ToByteArrayContent(v));
                response.EnsureSuccessStatusCode(); // not interested in testing it added it correctly, just that it succeeded
            }

            // Now let's get everything in the in-memory database and compare to what we added
            response = await _client.GetAsync("/vehicles");
            response.EnsureSuccessStatusCode();

            Assert.InRange((int)response.StatusCode, 200, 299); // Make sure the status code is success
            var respString = await response.Content.ReadAsStringAsync();
            var val = (List<Vehicle>)JsonConvert.DeserializeObject<List<Vehicle>>(respString);

            Assert.Equal(GoodVehicles.Count, val.Count); // Are input and output counts the same?

            // Check that all input vehicles are in the output vehicles
            foreach (Vehicle v in GoodVehicles)
            {
                Assert.True(val.Exists(AreEqual(v)));
            }
        }

        [Fact]
        public async void TestDeleteVehicle()
        {
            // Add one vehicle
            var v = GoodVehicles[0];
            var response = await _client.PostAsync("/vehicles", ToByteArrayContent(v));
            response.EnsureSuccessStatusCode(); 
            // let's get what was added to ensure we have the ID right
            var respString = await response.Content.ReadAsStringAsync();
            var val = (Vehicle)JsonConvert.DeserializeObject<Vehicle>(respString);

            Assert.InRange(val.Id, 1, Int32.MaxValue);

            // Now let's try to delete that vechicle
            response = await _client.DeleteAsync("/vehicles/" + val.Id);
            response.EnsureSuccessStatusCode();
            Assert.InRange((int)response.StatusCode, 200, 299); // Make sure the status code is success

            // Now let's get everything in the in-memory database and ensure it's empty
            response = await _client.GetAsync("/vehicles");
            response.EnsureSuccessStatusCode();
            respString = await response.Content.ReadAsStringAsync();
            var valList = (List<Vehicle>)JsonConvert.DeserializeObject<List<Vehicle>>(respString);

            Assert.Equal(0, valList.Count); // we should have gotten nothing
        }

        [Fact]
        public async void TestUpdateVehicle()
        {
            // Add one vehicle
            var v = GoodVehicles[0];
            var response = await _client.PostAsync("/vehicles", ToByteArrayContent(v));
            response.EnsureSuccessStatusCode(); 
            // let's get what was added to ensure we have the ID right
            var respString = await response.Content.ReadAsStringAsync();
            var val = (Vehicle)JsonConvert.DeserializeObject<Vehicle>(respString);

            Assert.InRange(val.Id, 1, Int32.MaxValue);

            // Now let's change the Model to bogus data
            val.Model = "Bogus";

            // Now let's try to update that vechicle
            response = await _client.PutAsync("/vehicles/" + val.Id, ToByteArrayContent(val));
            response.EnsureSuccessStatusCode();
            Assert.InRange((int)response.StatusCode, 200, 299); // Make sure the status code is success
            var input = val; // save what we sent for testing

            // Let's retrieve our vehicle to compare
            response = await _client.GetAsync("/vehicles/" + val.Id);
            response.EnsureSuccessStatusCode();
            respString = await response.Content.ReadAsStringAsync();
            val = (Vehicle)JsonConvert.DeserializeObject<Vehicle>(respString);

            // Did it add what we told it to?
            Assert.Equal(input, val, new VehicleComparer());
            // Just check that the ID generated was witin an expected range
            Assert.InRange(val.Id, 1, Int32.MaxValue);
        }

        private static Predicate<Vehicle> AreEqual(Vehicle y)
        {
            return delegate (Vehicle v) { return new VehicleComparer().Equals(v, y); };
        }

        protected ByteArrayContent ToByteArrayContent(Vehicle v)
        {
            var myContent = JsonConvert.SerializeObject(v);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            return byteContent;
        }
    }

    public class VehicleComparer : IEqualityComparer<Vehicle>
    {
        public bool Equals(Vehicle a, Vehicle b)
        {
            if (a == null && b == null)
                return true;
            else if (a == null || b == null)
                return false;

            return a.Year == b.Year &&
                a.Model.ToLowerInvariant() == b.Model.ToLowerInvariant() &&
                a.Make.ToLowerInvariant() == b.Make.ToLowerInvariant();
        }

        public int GetHashCode(Vehicle v)
        {
            return v.Year.GetHashCode() ^ v.Make.ToLowerInvariant().GetHashCode() ^ v.Model.ToLowerInvariant().GetHashCode();
        }
    }
}