namespace Speeder.Infra;

public class ServerPool(int retryTimeoutSeconds = 60)
{
    public class Iperf3Server 
    {
        public string Hostname { get; set; }

        public string PortRange { get; set; }

        public DateTime? UnavailableSince { get; set; }
    }

    private readonly List<Iperf3Server> _pool = new();
    private int _lastSelectedServerIdx = 0;

    public void AddServer(string hostname, string portRange) =>
        _pool.Add(new Iperf3Server { Hostname = hostname, PortRange = portRange });

    public void MarkUnavailable(Iperf3Server server)
    {
        if(_pool.Contains(server))
        {
            server.UnavailableSince = DateTime.UtcNow;
        }
    }

    public Iperf3Server? GetNextAvailable()
    {
        if (_pool.Count < 1) return null;

        if(++_lastSelectedServerIdx == _pool.Count) _lastSelectedServerIdx = 0;
        if(_lastSelectedServerIdx >= _pool.Count) return null;

        return _pool.Skip(_lastSelectedServerIdx - 1)
            .FirstOrDefault(s => s.UnavailableSince == null || s.UnavailableSince >= DateTime.UtcNow.AddMinutes(retryTimeoutSeconds));
    }
}

