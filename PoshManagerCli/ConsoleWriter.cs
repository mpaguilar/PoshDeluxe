using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PoshManager;

namespace PoshManagerCli
{
    public class ConsoleWriter : IPoshStream
    {

        public enum MessageType
        {
            Debug,
            Verbose,
            Warning,
            Error
        }

        public Action<String> VerboseWriter { get; set; }
        public Action<String> DebugWriter { get; set; }
        public Action<String> WarningWriter { get; set; }
        public Action<String> ErrorWriter { get; set; }

        public ConsoleWriter()
        {
            VerboseWriter = Writer(MessageType.Verbose);
            DebugWriter = Writer(MessageType.Debug);
            WarningWriter = Writer(MessageType.Warning);
            ErrorWriter = Writer(MessageType.Error);
        }

        public void Write(PoshManager.PoshMessage message) { }

        public Action<String> Formatter(
            String formatString = "{0}: {1}"
            )
        {
            return (str) =>
            {
                Console.WriteLine(formatString, DateTime.Now, str);
            };
        }
        
        public Action<String> ColorWrapper(ConsoleColor color, Action<String>logWriter)
        {
            return (str) =>
                {
                    ConsoleColor prev = Console.ForegroundColor;
                    Console.ForegroundColor = color;

                    logWriter(str);

                    Console.ForegroundColor = prev;
                };
        }

        /// <summary>
        /// The format string should accept two parameters
        /// it will be passed a DateTime and the message
        /// e.g. {0}: {1}
        /// </summary>
        /// <param name="formatString"></param>
        /// <returns></returns>
        public Action<String> Writer(MessageType t, String format = null)
        {
            Action<String> ret = null;

            if (null != format)
            {
                ret = ColorWrapper(Console.ForegroundColor, Formatter(format));
            }
            else
            {
                switch (t)
                {
                    case MessageType.Verbose:
                        ret = ColorWrapper(ConsoleColor.Cyan, Formatter("{0}: VERBOSE: {1}"));
                        break;

                    case MessageType.Error:
                        ret = ColorWrapper(ConsoleColor.Red, Formatter("{0}: ERROR: {1}"));
                        break;

                    case MessageType.Warning:
                        ret = ColorWrapper(ConsoleColor.Yellow, Formatter("{0}: WARNING: {1}"));
                        break;

                    case MessageType.Debug:
                        ret = ColorWrapper(ConsoleColor.Green, Formatter("{0}: DEBUG: {1}"));
                        break;

                    default:
                        ret = ColorWrapper(Console.ForegroundColor, Formatter("{0}: {1}"));
                        break;
                }
            }

            return ret;
        }
    }
}
