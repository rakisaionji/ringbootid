using System;

namespace RingBootId
{
    struct RingBootId
    {
        public string AppId;
        public string AppName;
        public RingAppInfo[] AppInfo;
    }

    struct RingAppInfo
    {
        public string AppId;
        public Version Version;
        public DateTime Date;

        public override string ToString()
        {
            return String.Format("{0} Ver.{1:0}.{2:00} {3:yyyy/MM/dd HH:mm:ss}", AppId, Version.Major, Version.Minor, Date);
        }
    }
}
