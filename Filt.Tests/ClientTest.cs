using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Filt.Tests
{
    [TestClass]
    public class ClientTest
    {
        [TestMethod]
        public async Task SendTest()
        {
            // param
            string url = Environment.GetEnvironmentVariable("Filt");
            byte[] target = System.Text.Encoding.ASCII.GetBytes("hello\n");

            System.Console.WriteLine("url:" + url);

            // send to filt
            FiltClient filt = new FiltClient(url);
            FiltRequest request = new FiltRequest(target);
            FiltResponse result = await filt.Send(request, verify:false);

            // print
            System.Console.WriteLine("hit:" + result.Hit);
            System.Console.WriteLine("sucess:" + result.Success);
            System.Console.WriteLine("message:" + System.Text.Encoding.ASCII.GetString(result.Messages[1]));

            throw new Exception();

        }
    }
}
