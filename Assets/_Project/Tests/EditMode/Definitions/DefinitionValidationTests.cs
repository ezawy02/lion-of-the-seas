using System.Linq;
using NUnit.Framework;
using SeaLion.Core.Definitions;

namespace SeaLion.Tests.EditMode.Definitions
{
    public sealed class DefinitionValidationTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Invalid Uppercase")]
        public void MissingOrMalformedIdsAreRejected(string rawValue)
        {
            var id = new StableId(rawValue);

            Assert.That(DefinitionValidation.ValidateId(id), Is.Not.Empty);
        }

        [Test]
        public void TryCreateDoesNotLeakRejectedValue()
        {
            var created = StableId.TryCreate("Invalid", out var id);

            Assert.That(created, Is.False);
            Assert.That(id, Is.EqualTo(StableId.Empty));
        }

        [Test]
        public void BrokenAndInvalidDefinitionReferencesAreReported()
        {
            var defined = new[] { Id("level-1"), Id("level-1"), new StableId("Invalid") };
            var references = new[] { Id("level-2") };

            var errors = DefinitionValidation.ValidateReferences(references, defined, "level");

            Assert.That(errors.Any(error => error.Contains("duplicated")), Is.True);
            Assert.That(errors.Any(error => error.Contains("invalid format")), Is.True);
            Assert.That(errors.Any(error => error.Contains("unresolved")), Is.True);
        }

        [TestCase(GateOutcome.Multiply, 0f)]
        [TestCase(GateOutcome.Add, -1f)]
        [TestCase(GateOutcome.Damage, 0f)]
        [TestCase(GateOutcome.Reward, -1f)]
        public void NonPositiveArithmeticGateValuesAreRejected(GateOutcome outcome, float value)
        {
            var errors = DefinitionValidation.ValidateGate(
                outcome, value, StableId.Empty, GateVisualStyle.Friendly);

            Assert.That(errors, Is.Not.Empty);
        }

        [Test]
        public void NonFiniteGateValueIsRejected()
        {
            var errors = DefinitionValidation.ValidateGate(
                GateOutcome.Multiply, float.NaN, StableId.Empty, GateVisualStyle.Friendly);

            Assert.That(errors.Any(error => error.Contains("finite")), Is.True);
        }

        [Test]
        public void ConvertRequiresAConversionIdAndOtherOutcomesForbidIt()
        {
            var missing = DefinitionValidation.ValidateGate(
                GateOutcome.Convert, 1f, StableId.Empty, GateVisualStyle.Special);
            var stray = DefinitionValidation.ValidateGate(
                GateOutcome.Add, 2f, Id("crew-role"), GateVisualStyle.Friendly);

            Assert.That(missing, Is.Not.Empty);
            Assert.That(stray, Is.Not.Empty);
        }

        [Test]
        public void PositiveGateCannotUseHostileVisualStyle()
        {
            var errors = DefinitionValidation.ValidateGate(
                GateOutcome.Multiply, 2f, StableId.Empty, GateVisualStyle.Hostile);

            Assert.That(errors.Any(error => error.Contains("hostile")), Is.True);
        }

        [Test]
        public void LoadoutRejectsUndefinedAndUnownedSelections()
        {
            var loadout = new LoadoutSnapshot(Id("ship-b"), Id("crew-a"), Id("ability-a"));
            var errors = DefinitionValidation.ValidateLoadout(
                loadout,
                new[] { Id("ship-a") },
                new[] { Id("crew-a") },
                new[] { Id("ability-a") },
                new[] { Id("crew-a") });

            Assert.That(errors.Any(error => error.Contains("flagshipId is undefined")), Is.True);
            Assert.That(errors.Any(error => error.Contains("flagshipId is not owned")), Is.True);
            Assert.That(errors.Any(error => error.Contains("captainAbilityId is not owned")), Is.True);
        }

        [Test]
        public void PhaseCycleIsRejected()
        {
            var phases = new[]
            {
                new PhaseLink(Id("opening"), Id("assault"), false),
                new PhaseLink(Id("assault"), Id("opening"), false)
            };

            var errors = DefinitionValidation.ValidatePhaseGraph(phases);

            Assert.That(errors.Any(error => error.Contains("cycle")), Is.True);
        }

        [Test]
        public void PhaseChainMustResolveAndEndAtTerminalPhase()
        {
            var valid = new[]
            {
                new PhaseLink(Id("opening"), Id("assault"), false),
                new PhaseLink(Id("assault"), Id("result"), false),
                new PhaseLink(Id("result"), StableId.Empty, true)
            };
            var broken = new[]
            {
                new PhaseLink(Id("opening"), Id("missing"), false)
            };

            Assert.That(DefinitionValidation.ValidatePhaseGraph(valid), Is.Empty);
            Assert.That(DefinitionValidation.ValidatePhaseGraph(broken), Is.Not.Empty);
        }

        private static StableId Id(string value)
        {
            Assert.That(StableId.TryCreate(value, out var id), Is.True);
            return id;
        }
    }
}
