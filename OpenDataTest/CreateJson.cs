using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace OpenDataTest
{
    class CreateJson
    {
        public List<debtData> GetJson(string url)
        {
            //string url = "http://data.ntpc.gov.tw/od/data/api/94D2B7F1-4AB9-4777-9EB5-9D922112EDD8?$format=json";
            
            return null;  
        }
        public float[] GetIntData(string[] data)
        {
            float[] value = new float[data.Length];
            for(int i = 0; i < data.Length; i++)
            {
                value[i] = float.Parse(data[i]);
            }
            return value;
        }
    }
}
