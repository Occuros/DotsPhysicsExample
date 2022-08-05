using Physics;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace DefaultNamespace
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(CollisionEventConversionSystem))]
    [UpdateBefore(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
    public partial class ChangeGravityOnCollisionSystem: SystemBase
    {

        private EntityCommandBufferSystem _commandBufferSystem;


        protected override void OnCreate()
        {
            base.OnCreate();
            _commandBufferSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _commandBufferSystem.CreateCommandBuffer();
            Entities.ForEach((Entity entity, in DynamicBuffer<StatefulCollisionEvent> collisions) =>
            {
                for (int i = 0; i < collisions.Length; i++)
                {
                    var collision = collisions[i];
                    if (collision.CollidingState != EventCollidingState.Enter) continue;
                    var otherEntity = collision.GetOtherEntity(entity);
                    if (!HasComponent<GravityChangerComponent>(otherEntity)) continue;
                    
                    if (!HasComponent<PhysicsGravityFactor>(entity))
                    {
                        ecb.AddComponent(entity, new PhysicsGravityFactor() {Value = -1});
                    }
                    else
                    {
                        var gravityFactor = GetComponent<PhysicsGravityFactor>(entity);
                        gravityFactor.Value *= -1;
                        SetComponent(entity, gravityFactor);
                    }

                    return;
                }
            }).Schedule();
            
            _commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}