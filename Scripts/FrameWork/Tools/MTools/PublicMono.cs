using System;

namespace MyGameNamespace
{
    public class PublicMono : MonoSingle<PublicMono>
    {
        public event Action OnUpdate;
        public event Action OnFixedUpdate;
        public event Action OnLateUpdate;
        public event Action OnGuiCall;

        private void Update()
        {
            OnUpdate?.Invoke();
        }
        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }
        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }
        private void OnGUI()
        {
            OnGuiCall?.Invoke();
        }
        protected override void DeInit()
        {
            OnUpdate = null;
            OnFixedUpdate = null;
            OnLateUpdate = null;
            OnGuiCall = null;
        }
        protected override void Init()
        {
            DontDestroyOnLoad(this);//过场景保留
        }
    }
}