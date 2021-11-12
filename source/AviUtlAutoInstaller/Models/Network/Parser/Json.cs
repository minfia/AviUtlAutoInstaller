using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlAutoInstaller.Models.Network.Parser
{
    class Json
    {
        private List<Dictionary<string, object>> parseResult = new List<Dictionary<string, object>>();

        public Json()
        {
        }

        /// <summary>
        /// JSONからJSON形式の辞書型に変換
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> Parse(string jsonText)
        {
            parseResult.Clear();

            JToken token = JsonConvert.DeserializeObject(jsonText) as JToken;

            Parse(token, parseResult);

            return parseResult;
        }

        /// <summary>
        /// トークンから解析
        /// </summary>
        /// <param name="token"></param>
        /// <param name="target">解析結果の格納先</param>
        private void Parse(JToken token, object target)
        {
            if (token.Type == JTokenType.Object)
            {
                ParseObject(token as JObject, target);
            }
            else if (token.Type == JTokenType.Array)
            {
                ParseArray(token as JArray, target);
            }
            else
            {
            }
        }

        /// <summary>
        /// JObjectを解析
        /// </summary>
        /// <param name="jObject"></param>
        /// <param name="target">解析結果の格納先</param>
        private void ParseObject(JObject jObject, object target)
        {
            foreach (var jProperty in jObject.Properties())
            {
                if (jProperty.Value.Type == JTokenType.Object)
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    ((Dictionary<string, object>)target).Add(jProperty.Name, obj);
                    Parse(jProperty.Value, obj);
                }
                else if (jProperty.Value.Type == JTokenType.Array)
                {
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    ((Dictionary<string, object>)target).Add(jProperty.Name, list);
                    Parse(jProperty.Value, list);
                }
                else if (target.GetType() == typeof(Dictionary<string, object>))
                {
                    ((Dictionary<string, object>)target).Add(jProperty.Name, jProperty.Value);
                }
                else if (target.GetType() == typeof(List<Dictionary<string, object>>))
                {
                    Dictionary<string, object> obj = new Dictionary<string, object>();
                    ((List<Dictionary<string, object>>)target).Add(obj);
                    Parse(jObject, obj);
                    break;
                }
            }
        }

        /// <summary>
        /// JArrayを解析
        /// </summary>
        /// <param name="jArray"></param>
        /// <param name="target">解析結果の格納先</param>
        private void ParseArray(JArray jArray, object target)
        {
            foreach (var value in jArray)
            {
                Parse(value, target);
            }
        }
    }
}
