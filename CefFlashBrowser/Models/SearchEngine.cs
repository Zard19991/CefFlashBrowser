﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CefFlashBrowser.Models
{
    static class SearchEngine
    {
        public enum Engine
        {
            Baidu,
            Bing,
            So360,
            Sogou
        }

        public static string GetUrl(string str, Engine e = Engine.Baidu)
        {
            switch (e)
            {
                case Engine.Baidu:
                    return $"www.baidu.com/s?wd={str}";

                case Engine.Bing:
                    return $"bing.com/search?q={str}";

                case Engine.Sogou:
                    return $"www.sogou.com/web?query={str}";

                case Engine.So360:
                    return $"www.so.com/s?&q={str}";

                default:
                    throw new Exception("Unknown search engine.");
            }
        }

        public static IEnumerable<(Engine engine, string name)> GetSupportedSearchEngines()
        {
            yield return (Engine.Baidu, LanguageManager.GetString("baidu"));
            yield return (Engine.Bing, LanguageManager.GetString("bing"));
            yield return (Engine.So360, LanguageManager.GetString("so360"));
            yield return (Engine.Sogou, LanguageManager.GetString("sogou"));
        }
    }
}
