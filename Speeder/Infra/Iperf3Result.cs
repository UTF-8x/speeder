namespace Speeder.Infra;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Iperf3Result
{
    [JsonPropertyName("start")]
    public Start Start { get; set; }

    [JsonPropertyName("intervals")]
    public List<Interval> Intervals { get; set; }

    [JsonPropertyName("end")]
    public End End { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class Start
{
    [JsonPropertyName("connected")]
    public List<Connected> Connected { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("system_info")]
    public string SystemInfo { get; set; }

    [JsonPropertyName("timestamp")]
    public Timestamp Timestamp { get; set; }

    [JsonPropertyName("connecting_to")]
    public ConnectingTo ConnectingTo { get; set; }

    [JsonPropertyName("cookie")]
    public string Cookie { get; set; }

    [JsonPropertyName("tcp_mss_default")]
    public int TcpMssDefault { get; set; }

    [JsonPropertyName("target_bitrate")]
    public int TargetBitrate { get; set; }

    [JsonPropertyName("fq_rate")]
    public int FqRate { get; set; }

    [JsonPropertyName("sock_bufsize")]
    public int SockBufsize { get; set; }

    [JsonPropertyName("sndbuf_actual")]
    public int SndbufActual { get; set; }

    [JsonPropertyName("rcvbuf_actual")]
    public int RcvbufActual { get; set; }

    [JsonPropertyName("test_start")]
    public TestStart TestStart { get; set; }
}

public class Connected
{
    [JsonPropertyName("socket")]
    public int Socket { get; set; }

    [JsonPropertyName("local_host")]
    public string LocalHost { get; set; }

    [JsonPropertyName("local_port")]
    public int LocalPort { get; set; }

    [JsonPropertyName("remote_host")]
    public string RemoteHost { get; set; }

    [JsonPropertyName("remote_port")]
    public int RemotePort { get; set; }
}

public class Timestamp
{
    [JsonPropertyName("time")]
    public string Time { get; set; }

    [JsonPropertyName("timesecs")]
    public int Timesecs { get; set; }
}

public class ConnectingTo
{
    [JsonPropertyName("host")]
    public string Host { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }
}

public class TestStart
{
    [JsonPropertyName("protocol")]
    public string Protocol { get; set; }

    [JsonPropertyName("num_streams")]
    public int NumStreams { get; set; }

    [JsonPropertyName("blksize")]
    public int Blksize { get; set; }

    [JsonPropertyName("omit")]
    public int Omit { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("bytes")]
    public int Bytes { get; set; }

    [JsonPropertyName("blocks")]
    public int Blocks { get; set; }

    [JsonPropertyName("reverse")]
    public int Reverse { get; set; }

    [JsonPropertyName("tos")]
    public int Tos { get; set; }

    [JsonPropertyName("target_bitrate")]
    public int TargetBitrate { get; set; }

    [JsonPropertyName("bidir")]
    public int Bidir { get; set; }

    [JsonPropertyName("fqrate")]
    public int Fqrate { get; set; }

    [JsonPropertyName("interval")]
    public int Interval { get; set; }
}

public class Interval
{
    [JsonPropertyName("streams")]
    public List<Stream> Streams { get; set; }

    [JsonPropertyName("sum")]
    public Sum Sum { get; set; }
}

public class Stream
{
    [JsonPropertyName("socket")]
    public int Socket { get; set; }

    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("end")]
    public double End { get; set; }

    [JsonPropertyName("seconds")]
    public double Seconds { get; set; }

    [JsonPropertyName("bytes")]
    public int Bytes { get; set; }

    [JsonPropertyName("bits_per_second")]
    public double BitsPerSecond { get; set; }

    [JsonPropertyName("omitted")]
    public bool Omitted { get; set; }

    [JsonPropertyName("sender")]
    public bool Sender { get; set; }
}

public class Sum
{
    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("end")]
    public double End { get; set; }

    [JsonPropertyName("seconds")]
    public double Seconds { get; set; }

    [JsonPropertyName("bytes")]
    public int Bytes { get; set; }

    [JsonPropertyName("bits_per_second")]
    public double BitsPerSecond { get; set; }

    [JsonPropertyName("omitted")]
    public bool Omitted { get; set; }

    [JsonPropertyName("sender")]
    public bool Sender { get; set; }
}

public class End
{
    [JsonPropertyName("streams")]
    public List<StreamEnd> Streams { get; set; }

    [JsonPropertyName("sum_sent")]
    public Sum SentSum { get; set; }

    [JsonPropertyName("sum_received")]
    public Sum ReceivedSum { get; set; }

    [JsonPropertyName("cpu_utilization_percent")]
    public CpuUtilization CpuUtilizationPercent { get; set; }

    [JsonPropertyName("receiver_tcp_congestion")]
    public string ReceiverTcpCongestion { get; set; }
}

public class StreamEnd
{
    [JsonPropertyName("sender")]
    public Stream Sender { get; set; }

    [JsonPropertyName("receiver")]
    public Stream Receiver { get; set; }
}

public class CpuUtilization
{
    [JsonPropertyName("host_total")]
    public double HostTotal { get; set; }

    [JsonPropertyName("host_user")]
    public double HostUser { get; set; }

    [JsonPropertyName("host_system")]
    public double HostSystem { get; set; }

    [JsonPropertyName("remote_total")]
    public double RemoteTotal { get; set; }

    [JsonPropertyName("remote_user")]
    public double RemoteUser { get; set; }

    [JsonPropertyName("remote_system")]
    public double RemoteSystem { get; set; }
}