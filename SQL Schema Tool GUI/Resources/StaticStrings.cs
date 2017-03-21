using System;
using System.Collections.Generic;
using System.Text;

namespace Lewis.SST.Resources
{
    public static class StaticStrings
    {
        //C:\Program Files\Sql Schema Tool\SST CommandLine\SSTCL.exe /src_server=llewis-lt\llewis2000 /src_catalog=aus4 /src_uid=sa /src_pwd=somepassword /dest_server=llewis-lt\llewis2000 /dest_catalog=uhc /dest_uid=sa /dest_pwd=somepassword /translate
        public static string sst_CommandLineCompare = @" {0} {1} {2} {3} {4} /Translate";
        public static string sst_CommandLineGenerate = @" {0} {1} {2} {3}";
        public static string sst_SourceSecurityPassword = @"/Src_uid={0} /Src_pwd={1}";
        public static string sst_DestSecurityPassword = @"/Dest_uid={0} /Dest_pwd={1}";
        public static string sst_SourceSecurityTrusted = @"/Src_Trusted";
        public static string sst_DestSecurityTrusted = @"/Dest_Trusted";
        public static string sst_SourceFile = @"/Src_File={0}";
        public static string sst_DestFile = @"/Dest_File={0}";
        public static string sst_SourceDatabase = @"/Src_server={0} /Src_catalog={1}";
        public static string sst_DestDatabase = @"/Dest_server={0} /Dest_catalog={1}";
        public static string sst_SourceSchemaSnapShot = @"/Src_Schema={0}";
        public static string sst_DestSchemaSnapShot = @"/Dest_Schema={0}";
    }
}
