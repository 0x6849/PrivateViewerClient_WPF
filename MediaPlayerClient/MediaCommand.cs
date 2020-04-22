using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MediaPlayerClient
{
    public class MediaCommand
    {
        public string roomID;
        public bool? paused;
        public double? timeStamp;
        public double? playSpeed;
        [JsonProperty("action")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Action action;
        public String name = "WPF";

        public MediaCommand(string roomId, bool? paused, double? timeStamp, double? playSpeed, Action action)
        {
            this.roomID = roomId;
            this.paused = paused;
            this.timeStamp = timeStamp;
            this.playSpeed = playSpeed;
            this.action = action;
        }

        public static MediaCommand Join(String room)
        {
            return new MediaCommand(room, null, null, null, Action.join);
        }

        public static MediaCommand Pause(bool pause)
        {
            return new MediaCommand(null, pause, null, null, Action.change);
        }

        public static MediaCommand SetTimeStamp(double timeStamp)
        {
            return new MediaCommand(null, null, timeStamp, null, Action.change);
        }

        public static MediaCommand SetPlaySpeed(double playSpeed)
        {
            return new MediaCommand(null, null, null, playSpeed, Action.change);
        }

        public static MediaCommand GetUpdate()
        {
            return new MediaCommand(null, null, null, null, Action.getUpdate);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}", roomID, paused, timeStamp, playSpeed, action);
        }
    }
}
