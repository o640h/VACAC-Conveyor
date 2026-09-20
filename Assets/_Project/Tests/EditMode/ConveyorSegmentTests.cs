using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace VACAC.Tests
{
    public class ConveyorSegmentTests
    {
        private readonly List<GameObject> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject createdObject in createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }

            createdObjects.Clear();
        }

        [Test]
        public void ConnectNext_ReplacesConnectionWithoutLeavingStaleLinks()
        {
            ConveyorSegment first = CreateSegment("First");
            ConveyorSegment originalNext = CreateSegment("Original Next");
            ConveyorSegment replacement = CreateSegment("Replacement");

            first.ConnectNext(originalNext);
            first.ConnectNext(replacement);

            Assert.That(first.NextSegment, Is.SameAs(replacement));
            Assert.That(replacement.PreviousSegment, Is.SameAs(first));
            Assert.That(originalNext.PreviousSegment, Is.Null);
        }

        [Test]
        public void FindFirstSegment_ReturnsBeginningOfConnectedChain()
        {
            ConveyorSegment first = CreateSegment("First");
            ConveyorSegment middle = CreateSegment("Middle");
            ConveyorSegment last = CreateSegment("Last");

            first.ConnectNext(middle);
            middle.ConnectNext(last);

            Assert.That(last.FindFirstSegment(), Is.SameAs(first));
            Assert.That(middle.FindFirstSegment(), Is.SameAs(first));
            Assert.That(first.FindFirstSegment(), Is.SameAs(first));
        }

        private ConveyorSegment CreateSegment(string name)
        {
            GameObject gameObject = new(name);
            createdObjects.Add(gameObject);
            return gameObject.AddComponent<ConveyorSegment>();
        }
    }
}
