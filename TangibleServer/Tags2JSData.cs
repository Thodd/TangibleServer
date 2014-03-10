using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TangibleServer
{
    [DataContract]
    class Tags2JSData
    {
        [DataMember]
        String eventType = "no_tag";
        [DataMember]
        long id = -1;
        [DataMember]
        long tagValue = -1;
        [DataMember]
        double x = 0;
        [DataMember]
        double y = 0;
        [DataMember]
        double rotation = 0;

        public Tags2JSData(String eventType, Microsoft.Surface.Core.TouchPoint tp)
        {
            if(tp.IsTagRecognized)
            {
                this.eventType = eventType;
                this.id = tp.Id;
                this.tagValue = tp.Tag.Value;
                this.x = tp.X;
                this.y = tp.Y;
                this.rotation = tp.Orientation;
            }
        }

        public String GetJSONString()
        {
            return JSON.JSONSerialize<Tags2JSData>(this);
        }
    }
}
