using System;
using UniRx;
using UnityEngine;

namespace Sample.Section2.Observables
{
    public class MessageSample : MonoBehaviour
    {
        [SerializeField] private float _countTimeSecounds = 30f;

        public IObservable<Unit> OnTimeUpAsyncSubject => _onTimeUpAsyncSubject;

        private readonly AsyncSubject<Unit> _onTimeUpAsyncSubject = new AsyncSubject<Unit>();

        private IDisposable _disposable;

        void Start()
        {
            _disposable = Observable.Timer(TimeSpan.FromSeconds(_countTimeSecounds))
                .Subscribe(_ =>
                {
                    _onTimeUpAsyncSubject.OnNext(Unit.Default);
                    _onTimeUpAsyncSubject.OnCompleted();
                });
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();

            _onTimeUpAsyncSubject.Dispose();
        }

        void Update()
        {

        }
    }
}