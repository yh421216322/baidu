using MyGameNamespace;
using UnityEngine;
using QFramework; // For IController, if needed, and for this.GetSystem/Model if used directly
using YourGameNamespace.Enemies; // For Zombie data class

namespace YourGameNamespace.Enemies // Or YourGameNamespace.View
{
    public class ZombieView : MonoBehaviour, IController // Implementing IController for consistency if it needs to interact with QF systems/models
    {
        public SpriteRenderer spriteRenderer; // 用于显示僵尸的2D图像，请在预制件中链接
        // public Animator animator; // 如果有动画控制器，请链接
        // public Collider2D collider2D; // 如果需要独立的碰撞体控制

        private Zombie mZombieData; // 关联的僵尸数据
        private IObjectPoolSystem mObjectPool; // 对象池系统，用于回收

        public IArchitecture GetArchitecture() => RegisterManager.Interface; // IController requirement

        void Awake()
        {
            
            
            
            if (spriteRenderer == null)
            {
                // 尝试自动获取，如果预制件结构固定（例如SpriteRenderer在根对象上）
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Debug.LogError("ZombieView: SpriteRenderer 未在Inspector中分配且无法自动获取！");
                }
            }
            // animator = GetComponent<Animator>(); // 类似处理
            // collider2D = GetComponent<Collider2D>(); // 类似处理
        }

        public void InitPool(IObjectPoolSystem pool)
        {
            mObjectPool = pool;
        }

        public void Setup(Zombie zombieData)
        {
            mZombieData = zombieData;
            this.gameObject.name = $"Zombie_{zombieData.Id.ToString().Substring(0, 4)}"; // 给GameObject一个有意义的名字

            // 根据zombieData更新初始视觉状态
            // 例如，如果ZombieData中有生命值百分比影响外观的逻辑，可以在此设置
            // spriteRenderer.color = Color.white; // 重置颜色
            // transform.localScale = Vector3.one; // 重置大小

            // 如果Zombie数据中的IsDead是BindableProperty，可以监听它来触发死亡表现
            // 但CombatSystem会在逻辑上处理死亡并发起回收，所以这里可能不需要重复监听
            // mZombieData.IsDead.Register(isDead => { if (isDead) OnDeath(); }).UnRegisterWhenGameObjectDestroyed(this);

            UpdatePosition(); // 设置初始位置
        }

        void Update()
        {
            if (mZombieData == null || mZombieData.IsDead.Value) // 确保有数据且未死亡
            {
                // 如果已死亡，可以停止移动或做其他处理，但通常回收前会被禁用
                return;
            }

            // 移动逻辑: ZombieView 负责根据其 mZombieData 中的目标位置来更新自己的 transform.position
            // CombatSystem 仍然负责更新 mZombieData.TargetPosition 和 mZombieData.Position (逻辑位置)
            // ZombieView 只是将逻辑位置“表现”出来
            if (mZombieData.Position != (Vector2)transform.position) // 简单同步，或使用更平滑的插值移动
            {
                 UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            if(mZombieData != null)
            {
                // 可以直接设置，或者使用 Vector2.MoveTowards/Lerp 进行平滑移动
                transform.position = mZombieData.Position;
            }
        }

        // 由CombatSystem在确认僵尸逻辑死亡后调用
        public void TriggerDeathSequenceAndRecycle()
        {
            // 播放死亡动画 (如果有)
            // if (animator != null) animator.SetTrigger("Die");
            Debug.Log($"{this.gameObject.name} 正在播放死亡动画并准备被回收...");

            // 延迟回收，给动画播放时间 (如果需要)
            // 或者动画结束时通过事件回调来回收
            // StartCoroutine(RecycleAfterDelay(1.0f)); // 假设死亡动画1秒

            // 为简单起见，暂时立即回收
            RecycleSelf();
        }

        // private System.Collections.IEnumerator RecycleAfterDelay(float delay)
        // {
        //     yield return new WaitForSeconds(delay);
        //     RecycleSelf();
        // }

        private void RecycleSelf()
        {
            if (mObjectPool != null)
            {
                mObjectPool.Unspawn(this.gameObject); // 使用Unspawn (之前修正的拼写)
            }
            else
            {
                // 如果没有对象池（例如直接Instantiate的），则直接销毁
                Debug.LogWarning($"ZombieView {this.gameObject.name} 没有正确设置对象池，将直接销毁。");
                Destroy(this.gameObject);
            }
        }

        // 可选：如果僵尸有攻击动画，CombatSystem在判断僵尸攻击时可以调用这个
        public void PlayAttackAnimation()
        {
            // if (animator != null) animator.SetTrigger("Attack");
            Debug.Log($"{this.gameObject.name} 正在播放攻击动画。");
        }
    }
}
