using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace Semplicita.Helpers
{
    public static class Util
    {
        public static T GetSetting<T>( string name ) {
            string value = System.Configuration.ConfigurationManager.AppSettings[name];

            if ( value == null ) {
                throw new System.Exception( string.Format( "Could not find setting '{0}',", name ) );
            }

            return (T)System.Convert.ChangeType( value, typeof( T ), System.Globalization.CultureInfo.InvariantCulture );
        }

        public static string GetSetting( string name ) {
            string value = System.Configuration.ConfigurationManager.AppSettings[name];

            if ( value == null ) {
                throw new System.Exception( string.Format( "Could not find setting '{0}',", name ) );
            }

            return (string)System.Convert.ChangeType( value, typeof( string ), System.Globalization.CultureInfo.InvariantCulture );
        }

        public static string URLFriendly( string title ) {
            if ( title == null ) return "";
            const int maxlen = 80;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            char c;
            for ( int i = 0; i < len; i++ ) {
                c = title[i];
                if ( ( c >= 'a' && c <= 'z' ) || ( c >= '0' && c <= '9' ) ) {
                    sb.Append( c );
                    prevdash = false;
                } else if ( c >= 'A' && c <= 'Z' ) {
                    // tricky way to convert to lowercase
                    sb.Append( (char)( c | 32 ) );
                    prevdash = false;
                } else if ( c == ' ' || c == ',' || c == '.' || c == '/' ||
                           c == '\\' || c == '-' || c == '_' || c == '=' ) {
                    if ( !prevdash && sb.Length > 0 ) {
                        sb.Append( '-' );
                        prevdash = true;
                    }
                } else if ( c == '#' ) {
                    if ( i > 0 )
                        if ( title[i - 1] == 'C' || title[i - 1] == 'F' )
                            sb.Append( "-sharp" );
                } else if ( c == '+' ) {
                    sb.Append( "-plus" );
                } else if ( (int)c >= 128 ) {
                    int prevlen = sb.Length;
                    sb.Append( RemapInternationalCharToAscii( c ) );
                    if ( prevlen != sb.Length ) prevdash = false;
                }
                if ( sb.Length == maxlen ) break;
            }
            if ( prevdash )
                return sb.ToString().Substring( 0, sb.Length - 1 );
            else
                return sb.ToString();
        }

        private static string RemapInternationalCharToAscii( char c ) {
            string s = c.ToString().ToLowerInvariant();
            if ( "àåáâäãåą".Contains( s ) ) {
                return "a";
            } else if ( "èéêëę".Contains( s ) ) {
                return "e";
            } else if ( "ìíîïı".Contains( s ) ) {
                return "i";
            } else if ( "òóôõöøőð".Contains( s ) ) {
                return "o";
            } else if ( "ùúûüŭů".Contains( s ) ) {
                return "u";
            } else if ( "çćčĉ".Contains( s ) ) {
                return "c";
            } else if ( "żźž".Contains( s ) ) {
                return "z";
            } else if ( "śşšŝ".Contains( s ) ) {
                return "s";
            } else if ( "ñń".Contains( s ) ) {
                return "n";
            } else if ( "ýÿ".Contains( s ) ) {
                return "y";
            } else if ( "ğĝ".Contains( s ) ) {
                return "g";
            } else if ( c == 'ř' ) {
                return "r";
            } else if ( c == 'ł' ) {
                return "l";
            } else if ( c == 'đ' ) {
                return "d";
            } else if ( c == 'ß' ) {
                return "ss";
            } else if ( c == 'Þ' ) {
                return "th";
            } else if ( c == 'ĥ' ) {
                return "h";
            } else if ( c == 'ĵ' ) {
                return "j";
            } else {
                return "";
            }
        }

        public static List<string> CloneStringList( List<string> input ) {
            var output = new List<string>();
            foreach ( var i in input ) {
                output.Add( i );
            }
            return output;
        }

        //Toast extension methods to TempData
        private static int _successnum = 0, _dangernum = 0, _infonum = 0;

        public static void AddSuccessToast( this TempDataDictionary TempData, string input ) {
            TempData.Add( "tsuc-" + _successnum.ToString(), input );
            _successnum += 1;
        }

        public static void AddDangerToast( this TempDataDictionary TempData, string input ) {
            TempData.Add( "tdngr-" + _successnum.ToString(), input );
            _dangernum += 1;
        }

        public static void AddInfoToast( this TempDataDictionary TempData, string input ) {
            TempData.Add( "tinfo-" + _successnum.ToString(), input );
            _infonum += 1;
        }
    }
}