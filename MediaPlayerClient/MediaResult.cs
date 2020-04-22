using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MediaPlayerClient
{
    public class MediaResult : MediaCommand
    {
        public string result;
        public string message;
        public string[] rooms;

        public MediaResult(string roomId, bool? paused, double? timeStamp, double? playSpeed, Action action) : base(roomId, paused, timeStamp, playSpeed, action)
        {
        }

        public static MediaResult FromJson(string json)
        {
            return (MediaResult)JsonConvert.DeserializeObject(json, typeof(MediaResult));
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}: {3}", base.ToString(), result, rooms != null ? string.Join(", ", rooms) : null, message);
        }

        public bool isOk()
        {
            return "ok".Equals(result);
        }
    }
}
