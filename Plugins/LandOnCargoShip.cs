using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Land On Cargo Ship", "Arainrr", "1.0.1")]
    [Description("Allow the mini copter to land on the cargo ship")]
    public class LandOnCargoShip : RustPlugin
    {
        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));
        }

        private void OnServerInitialized()
        {
            Subscribe(nameof(OnEntitySpawned));
            foreach (var serverEntity in BaseNetworkable.serverEntities)
            {
                var miniCopter = serverEntity as MiniCopter;
                if (miniCopter != null)
                {
                    OnEntitySpawned(miniCopter);
                    continue;
                }
                var cargoShip = serverEntity as CargoShip;
                if (cargoShip != null)
                {
                    OnEntitySpawned(cargoShip);
                }
            }
        }

        private void OnEntitySpawned(MiniCopter miniCopter)
        {
            if (miniCopter == null) return;
            var parentTrigger = new GameObject("ParentTrigger");
            parentTrigger.transform.SetParent(miniCopter.transform);
            parentTrigger.transform.position = miniCopter.transform.position;
            var sphereCollider = parentTrigger.gameObject.AddComponent<SphereCollider>();
            sphereCollider.gameObject.layer = (int)Rust.Layer.Reserved1;
            sphereCollider.radius = 1.5f;
            sphereCollider.isTrigger = true;
        }

        private void OnEntitySpawned(CargoShip cargoShip)
        {
            if (cargoShip == null) return;
            var triggerParent = cargoShip.GetComponentInChildren<TriggerParent>();
            if (triggerParent == null) return;
            triggerParent.interestLayers |= (1 << (int)Rust.Layer.Reserved1);
        }

        private void Unload()
        {
            foreach (var serverEntity in BaseNetworkable.serverEntities)
            {
                var miniCopter = serverEntity as MiniCopter;
                if (miniCopter != null)
                {
                    var child = miniCopter.transform.Find("ParentTrigger");
                    if (child != null)
                    {
                        UnityEngine.Object.Destroy(child.gameObject);
                    }
                    continue;
                }
                var cargoShip = serverEntity as CargoShip;
                if (cargoShip != null)
                {
                    var triggerParent = cargoShip.GetComponentInChildren<TriggerParent>();
                    if (triggerParent != null) triggerParent.interestLayers &= ~(1 << (int)Rust.Layer.Reserved1);
                }
            }
        }
    }
}