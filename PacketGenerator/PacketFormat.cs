using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator {
    class PacketFormat {
        public static string packetPackageFormatClient =
@"
using MessagePack;
using System.Collections.Generic;

public enum PacketID {{{0}
}}

{1}public interface IPacket {{

}}

{2}
";
        public static string packetPackageFormat =
@"
using MessagePack;

public enum PacketID {{{0}
}}

{1}public interface IPacket {{

}}

{2}
";
        public static string packetIDFormat =
@"
    {0} = {1},";
        public static string unionFormat =
@"[MessagePack.Union({1}, typeof({0}))]
";
        public static string packetFormat =
@"[MessagePackObject]
public class {0} : IPacket {{{1}
}}

";
        public static string keyFormat = 
@"
    [Key({0})]
    {1}
";
    }
}
