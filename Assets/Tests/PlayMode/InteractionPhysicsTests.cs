using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Game.Interaction.PlayMode.Tests
{
    public class InteractionPhysicsTests
    {
        readonly List<UnityEngine.Object> cleanup = new List<UnityEngine.Object>();
        static readonly Assembly GameAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .First(assembly => assembly.GetName().Name == "Assembly-CSharp");

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var item in cleanup) if (item != null) UnityEngine.Object.Destroy(item);
            cleanup.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator DroppedCarryableLandsAboveFloorAndRestsInThreeLayouts()
        {
            for (int scenario = 0; scenario < 3; scenario++)
            {
                CreateFloor();
                if (scenario == 1) CreateBox(new Vector3(1.25f, 1f, 1.5f), new Vector3(0.3f, 2f, 2f));
                if (scenario == 2) CreateBox(new Vector3(0f, 0.25f, 1.6f), new Vector3(3f, 0.5f, 3f));
                Component player = CreatePlayer(new Vector3(0f, 1f, 0f));
                Component item = CreateCarryable(new Vector3(0f, 2f, 0f));
                Assert.That(Invoke<bool>(item, "PickUp", player), Is.True);
                Assert.That(Invoke<bool>(item, "Release", player, false), Is.True);

                for (int frame = 0; frame < 120; frame++) yield return null;
                var collider = item.GetComponent<CapsuleCollider>();
                var body = item.GetComponent<Rigidbody>();
                Assert.That(collider.bounds.min.y, Is.GreaterThanOrEqualTo(scenario == 2 ? 0.5f : 0f));
                Assert.That(body.linearVelocity.magnitude, Is.LessThan(0.1f));
                Assert.That(body.IsSleeping() || body.linearVelocity.magnitude < 0.1f, Is.True);
                foreach (var itemToDestroy in cleanup) if (itemToDestroy != null) UnityEngine.Object.Destroy(itemToDestroy);
                cleanup.Clear();
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator DiagonalGripKeepsBlockUprightAndSocketOffset()
        {
            CreateFloor();
            Component player = CreatePlayer(new Vector3(0f, 1f, -1.2f));
            Component block = CreateBlock(new Vector3(0f, 0.5f, 0f));
            Quaternion initialRotation = block.transform.rotation;
            Assert.That(Invoke<bool>(block, "TryBeginGrip", player), Is.True);
            for (int frame = 0; frame < 24; frame++) yield return null;

            Vector3 initialOffset = player.transform.position - block.transform.position;
            var controller = player.GetComponent<CharacterController>();
            var collider = block.GetComponent<BoxCollider>();
            for (int frame = 0; frame < 120; frame++)
            {
                Invoke<bool>(block, "MoveGrip", player, new Vector2(1f, 1f), Time.deltaTime);
                Assert.That(Physics.GetIgnoreCollision(controller, collider), Is.False);
                yield return null;
            }

            Assert.That(Mathf.Abs(Mathf.DeltaAngle(initialRotation.eulerAngles.y, block.transform.eulerAngles.y)), Is.GreaterThan(45f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(block.transform.eulerAngles.y, 90f)), Is.LessThan(5f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(block.transform.eulerAngles.x, 0f)), Is.LessThan(0.1f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(block.transform.eulerAngles.z, 0f)), Is.LessThan(0.1f));
            Assert.That((player.transform.position - block.transform.position - initialOffset).magnitude, Is.LessThan(0.08f));
            Assert.That((player.transform.position - new Vector3(0f, 1f, -1.2f)).magnitude, Is.GreaterThan(0.5f));
        }

        [UnityTest]
        public IEnumerator ThrownCarryableLeavesPlayerAndLandsAboveFloor()
        {
            CreateFloor();
            Component player = CreatePlayer(new Vector3(0f, 1f, 0f));
            Component item = CreateCarryable(new Vector3(0f, 2f, 0f));
            Assert.That(Invoke<bool>(item, "PickUp", player), Is.True);
            Assert.That(Invoke<bool>(item, "Release", player, true), Is.True);
            Vector3 thrownFrom = player.transform.position;
            for (int frame = 0; frame < 120; frame++) yield return null;
            var collider = item.GetComponent<CapsuleCollider>();
            var body = item.GetComponent<Rigidbody>();
            Assert.That(Vector3.Distance(item.transform.position, thrownFrom), Is.GreaterThan(1f));
            Assert.That(collider.bounds.min.y, Is.GreaterThanOrEqualTo(0f));
            Assert.That(body.linearVelocity.magnitude, Is.LessThan(0.15f));
        }

        GameObject CreateFloor()
        {
            var floor = new GameObject("Test Floor");
            cleanup.Add(floor);
            floor.transform.position = new Vector3(0f, -0.25f, 0f);
            floor.transform.localScale = new Vector3(30f, 0.5f, 30f);
            floor.AddComponent<BoxCollider>();
            return floor;
        }

        GameObject CreateBox(Vector3 position, Vector3 size)
        {
            var box = new GameObject("Test Support");
            cleanup.Add(box);
            box.transform.position = position;
            box.transform.localScale = size;
            box.AddComponent<BoxCollider>();
            return box;
        }

        Component CreatePlayer(Vector3 position)
        {
            var go = new GameObject("Test Player");
            cleanup.Add(go);
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<CharacterController>();
            var batteryConfig = NewAsset("Game.Player.BatteryConfig");
            var battery = go.AddComponent(TypeOf("Game.Player.Battery"));
            Set(battery, "config", batteryConfig);
            var stats = NewAsset("Game.Player.PlayerStats");
            var controller = go.AddComponent(TypeOf("Game.Player.PlayerController"));
            Set(controller, "stats", stats);
            Set(controller, "move", ActionReference("Move", InputActionType.Value));
            Set(controller, "run", ActionReference("Run", InputActionType.Button));
            Set(controller, "dash", ActionReference("Dash", InputActionType.Button));
            go.SetActive(true);
            return controller;
        }

        Component CreateCarryable(Vector3 position)
        {
            var go = new GameObject("Test Carryable");
            cleanup.Add(go);
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<Rigidbody>();
            go.AddComponent<CapsuleCollider>();
            var config = NewAsset("Game.Puzzle.CarryConfig");
            Set(config, "mass", 0.25f);
            Set(config, "dropSearchRadius", 0.8f);
            Set(config, "killPlaneY", -100f);
            var item = go.AddComponent(TypeOf("Game.Puzzle.CarryableItem"));
            Set(item, "config", config);
            go.SetActive(true);
            return item;
        }

        Component CreateBlock(Vector3 position)
        {
            var go = new GameObject("Test Block");
            cleanup.Add(go);
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<Rigidbody>();
            go.AddComponent<BoxCollider>();
            var config = NewAsset("Game.Puzzle.BlockConfig");
            Set(config, "mass", 0.5f);
            Set(config, "snapDuration", 0.05f);
            Set(config, "killPlaneY", -100f);
            var block = go.AddComponent(TypeOf("Game.Puzzle.PushPullBlock"));
            Set(block, "config", config);
            go.SetActive(true);
            return block;
        }

        UnityEngine.Object NewAsset(string typeName)
        {
            var asset = ScriptableObject.CreateInstance(TypeOf(typeName));
            cleanup.Add(asset);
            return asset;
        }

        InputActionReference ActionReference(string name, InputActionType type)
        {
            var action = new InputAction(name, type);
            var reference = InputActionReference.Create(action);
            cleanup.Add(reference);
            return reference;
        }

        static Type TypeOf(string name) => GameAssembly.GetType(name, true);

        static void Set(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(target, value);

        static T Invoke<T>(object target, string name, params object[] args) =>
            (T)target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(target, args);
    }
}
