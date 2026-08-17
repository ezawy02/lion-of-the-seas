using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using SeaLion.Crowd.Simulation;
using SeaLion.Core.Definitions;

namespace SeaLion.Tests.EditMode.Crowd
{
    public sealed class CrowdBuffersTests
    {
        [Test]
        public void ConstructorAllocatesSoaAtCapacity()
        {
            var buffers = new CrowdBuffers(4, Allocator.Persistent);
            try
            {
                Assert.That(buffers.Capacity, Is.EqualTo(4));
                Assert.That(buffers.Positions.IsCreated, Is.True);
                Assert.That(buffers.Velocities.IsCreated, Is.True);
                Assert.That(buffers.HealthOrHits.IsCreated, Is.True);
                Assert.That(buffers.Roles.IsCreated, Is.True);
                Assert.That(buffers.States.IsCreated, Is.True);
                Assert.That(buffers.Flags.IsCreated, Is.True);
                Assert.That(buffers.LogicalCount, Is.Zero);
            }
            finally { buffers.Dispose(); }
        }

        [Test]
        public void InitializeAndResetClearStateAndCounts()
        {
            var buffers = new CrowdBuffers(2, Allocator.TempJob);
            try
            {
                buffers.Initialize(2);
                buffers.Positions[0] = new float3(2, 3, 4);
                buffers.HealthOrHits[0] = 12f;
                buffers.Flags[0] = CrowdAgentFlags.HitQueued;
                buffers.SetDisplayedAgentCount(1);
                buffers.Reset();
                Assert.That(buffers.LogicalCount, Is.Zero);
                Assert.That(buffers.ActiveCount, Is.Zero);
                Assert.That(buffers.DisplayedAgentCount, Is.Zero);
                Assert.That(buffers.Positions[0], Is.EqualTo(float3.zero));
                Assert.That(buffers.HealthOrHits[0], Is.Zero);
                Assert.That(buffers.Flags[0], Is.EqualTo(CrowdAgentFlags.None));
            }
            finally { buffers.Dispose(); }
        }

        [Test]
        public void CountsAndIndicesRejectOutOfBounds()
        {
            var buffers = new CrowdBuffers(3, Allocator.TempJob);
            try
            {
                Assert.Throws<System.ArgumentOutOfRangeException>(() => buffers.Initialize(-1));
                buffers.Initialize(2);
                Assert.Throws<System.ArgumentOutOfRangeException>(() => buffers.SetActiveCount(3));
                Assert.Throws<System.ArgumentOutOfRangeException>(() => buffers.SetDisplayedAgentCount(3));
                Assert.Throws<System.ArgumentOutOfRangeException>(() => buffers.ValidateIndex(-1));
                Assert.Throws<System.ArgumentOutOfRangeException>(() => buffers.ValidateIndex(3));
            }
            finally { buffers.Dispose(); }
        }

        [Test]
        public void DisposeIsIdempotentAndGuardsUse()
        {
            var buffers = new CrowdBuffers(1, Allocator.Persistent);
            buffers.Dispose();
            Assert.DoesNotThrow(() => buffers.Dispose());
            Assert.That(buffers.IsDisposed, Is.True);
            Assert.That(buffers.Capacity, Is.Zero);
            Assert.Throws<System.ObjectDisposedException>(() => buffers.Initialize(0));
            Assert.Throws<System.ObjectDisposedException>(() => buffers.ValidateIndex(0));
        }

        [Test]
        public void LogicalAndPresentationCountsRemainSeparate()
        {
            var buffers = new CrowdBuffers(8, Allocator.TempJob);
            try
            {
                buffers.Initialize(8);
                buffers.SetActiveCount(5);
                buffers.SetDisplayedAgentCount(3);
                Assert.That(buffers.LogicalCount, Is.EqualTo(8));
                Assert.That(buffers.ActiveCount, Is.EqualTo(5));
                Assert.That(buffers.DisplayedAgentCount, Is.EqualTo(3));
                buffers.SetLogicalCount(2);
                Assert.That(buffers.ActiveCount, Is.EqualTo(2));
                Assert.That(buffers.DisplayedAgentCount, Is.EqualTo(2));
            }
            finally { buffers.Dispose(); }
        }

        [Test]
        public void LogicalCountMayExceedSimulationAndPresentationCapacity()
        {
            var buffers = new CrowdBuffers(3, Allocator.TempJob);
            try
            {
                buffers.Initialize(500);
                Assert.That(buffers.LogicalCount, Is.EqualTo(500));
                Assert.That(buffers.ActiveCount, Is.EqualTo(3));
                Assert.That(buffers.DisplayedAgentCount, Is.EqualTo(3));
                Assert.Throws<System.ArgumentOutOfRangeException>(() => buffers.SetDisplayedAgentCount(4));
            }
            finally { buffers.Dispose(); }
        }
    }
}
