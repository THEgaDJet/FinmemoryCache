namespace GenericInMemoryCache.Classes
{
    internal sealed class Unsubscriber<EvictionNotification> : IDisposable
    {
        private readonly ISet<IObserver<EvictionNotification>> _observers;
        private readonly IObserver<EvictionNotification> _observer;

        internal Unsubscriber(
            ISet<IObserver<EvictionNotification>> observers,
            IObserver<EvictionNotification> observer) => (_observers, _observer) = (observers, observer);

        public void Dispose() => _observers.Remove(_observer);
    }
}