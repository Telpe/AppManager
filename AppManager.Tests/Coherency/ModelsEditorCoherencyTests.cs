using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppManager.Core.Actions;
using AppManager.Core.Triggers;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using System;
using System.Linq;
using System.Reflection;

namespace AppManager.Tests.Coherency
{
    [TestClass]
    public class ModelsEditorCoherencyTests
    {
        private int ActionScratchLength = nameof(IAction)[1..].Length;
        private int TriggerScratchLength = nameof(ITrigger)[1..].Length;
        private int ConditionScratchLength = nameof(ICondition)[1..].Length;
        private static string[] _existingActionTypes = [];
        private static string[] _actualTriggerTypes = [];
        private static string[] _actualConditionTypes = [];
        private static ActionTypeEnum[] _supportedActionTypes = [];
        private static TriggerTypeEnum[] _supportedTriggerTypes = [];
        private static ConditionTypeEnum[] _supportedConditionTypes = [];
        private static ActionTypeEnum[] _availableActionTypes = [];
        private static TriggerTypeEnum[] _availableTriggerTypes = [];
        private static ConditionTypeEnum[] _availableConditionTypes = [];

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Supported types are those in the enums
            _supportedActionTypes = Enum.GetValues<ActionTypeEnum>();
            _supportedTriggerTypes = Enum.GetValues<TriggerTypeEnum>();
            _supportedConditionTypes = Enum.GetValues<ConditionTypeEnum>();

            // Available types are those returned by the factories as supported
            _availableActionTypes = ActionFactory.GetSupportedActionTypes().ToArray();
            _availableTriggerTypes = TriggerFactory.GetSupportedTriggerTypes().ToArray();
            _availableConditionTypes = ConditionFactory.GetSupportedConditionTypes().ToArray();

            // Actual types are the classes found via reflection in the core assembly
            var coreAssembly = Assembly.GetAssembly(typeof(IAction));
            if (coreAssembly is not null)
            {
                foreach (var type in coreAssembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract)
                    {
                        if (typeof(IAction).IsAssignableFrom(type))
                        {
                            _existingActionTypes = [.._existingActionTypes, type.Name];
                        }
                        else if (typeof(ITrigger).IsAssignableFrom(type))
                        {
                            _actualTriggerTypes = [.._actualTriggerTypes, type.Name];
                        }
                        else if (typeof(ICondition).IsAssignableFrom(type))
                        {
                            _actualConditionTypes = [.._actualConditionTypes, type.Name];
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void AvailableActionsAreSupported()
        {
            string[] notSupported = AvailableAreSupported(_availableActionTypes.Select(a => a.ToString()).ToArray(), _supportedActionTypes.Select(a => a.ToString()).ToArray() );
            
            Assert.IsLessThan(1, notSupported.Length,
                $"\n{nameof(ActionFactory)} does not support: \n{String.Join('\n', notSupported)}, \nthough set available by {nameof(ActionTypeEnum)}.");
        }

        [TestMethod]
        public void SupportedActionsActuallyExists()
        {
            string[] notExisting = SupportedActuallyExists(_supportedActionTypes.Select(a => a.ToString()+"Action").ToArray(), _existingActionTypes);

            Assert.IsLessThan(1, notExisting.Length,
                    $"\nAction{(1 < notExisting.Length ?"s":"")} missing are: \n{String.Join('\n', notExisting)}, \nthough supported by {nameof(ActionFactory)}");
        }

        [TestMethod]
        public void ExistingActionsAreAvailable()
        {
            string[] unavailableActions = ExistingAreAvailable(_existingActionTypes, typeof(ActionTypeEnum), ActionScratchLength);

            if (0 < unavailableActions.Length) 
            { 
                Assert.Inconclusive($"\n⚠️ Actions not available: \n{String.Join('\n', unavailableActions)}. \nConsider adding them to {nameof(ActionTypeEnum)}, if they are meant to be supported and available.");
            }
        }

        [TestMethod]
        public void ActionModelImplementsRequiredInterfaces()
        {
            string[] missingInterfaces = ModelImplementsRequiredInterfaces(_existingActionTypes, typeof(ActionModel));

            Assert.IsLessThan(1, missingInterfaces.Length,
                $"\n{nameof(ActionModel)} does not implement all required interfaces for: \n{String.Join('\n', missingInterfaces)}.");
        }

        [TestMethod]
        public void ActionModelHasUselessInterfaces()
        {
            string[] extraInterfaces = ModelHasUselessInterfaces(typeof(ActionModel), typeof(ActionTypeEnum), ActionScratchLength);

            foreach (string actionInterface in extraInterfaces)
            {
                Console.WriteLine($"⚠️ Warning, unused interface{(0<extraInterfaces.Length?"s":"")} in {nameof(ActionModel)}: \n{String.Join('\n', extraInterfaces)}.");
            }
        }

        [TestMethod]
        public void AvailableTriggersAreSupported()
        {
            string[] notSupported = AvailableAreSupported(_availableTriggerTypes.Select(a => a.ToString()).ToArray(), _supportedTriggerTypes.Select(a => a.ToString()).ToArray() );
            
            Assert.IsLessThan(1, notSupported.Length,
                $"\n{nameof(TriggerFactory)} does not support: \n{String.Join('\n', notSupported)}, \nthough set available by {nameof(TriggerTypeEnum)}.");
        }

        [TestMethod]
        public void SupportedTriggersActuallyExists()
        {
            string[] notExisting = SupportedActuallyExists(_supportedTriggerTypes.Select(a => a.ToString()+"Trigger").ToArray(), _actualTriggerTypes);

            Assert.IsLessThan(1, notExisting.Length,
                    $"\nTrigger{(1 < notExisting.Length ?"s":"")} missing are: \n{String.Join('\n', notExisting)}, \nthough supported by {nameof(TriggerFactory)}");
        }

        [TestMethod]
        public void ExistingTriggersAreAvailable()
        {
            string[] unavailableTriggers = ExistingAreAvailable(_actualTriggerTypes, typeof(TriggerTypeEnum), TriggerScratchLength);

            if (0 < unavailableTriggers.Length) 
            { 
                Assert.Inconclusive($"\n⚠️ Triggers not available: \n{String.Join('\n', unavailableTriggers)}. \nConsider adding them to {nameof(TriggerTypeEnum)}, if they are meant to be supported and available.");
            }
        }

        [TestMethod]
        public void TriggerModelImplementsRequiredInterfaces()
        {
            string[] missingInterfaces = ModelImplementsRequiredInterfaces(_actualTriggerTypes, typeof(TriggerModel));

            Assert.IsLessThan(1, missingInterfaces.Length,
                $"\n{nameof(TriggerModel)} does not implement all required interfaces for: \n{String.Join('\n', missingInterfaces)}.");
        }

        [TestMethod]
        public void TriggerModelHasUselessInterfaces()
        {
            string[] extraInterfaces = ModelHasUselessInterfaces(typeof(TriggerModel), typeof(TriggerTypeEnum), TriggerScratchLength);

            foreach (string triggerInterface in extraInterfaces)
            {
                Console.WriteLine($"⚠️ Warning, unused interface{(0<extraInterfaces.Length?"s":"")} in {nameof(TriggerModel)}: \n{String.Join('\n', extraInterfaces)}.");
            }
        }

        [TestMethod]
        public void AvailableConditionsAreSupported()
        {
            string[] notSupported = AvailableAreSupported(_availableConditionTypes.Select(a => a.ToString()).ToArray(), _supportedConditionTypes.Select(a => a.ToString()).ToArray() );
            
            Assert.IsLessThan(1, notSupported.Length,
                $"\n{nameof(ConditionFactory)} does not support: \n{String.Join('\n', notSupported)}, \nthough set available by {nameof(ConditionTypeEnum)}.");
        }

        [TestMethod]
        public void SupportedConditionsActuallyExists()
        {
            string[] notExisting = SupportedActuallyExists(_supportedConditionTypes.Select(a => a.ToString()+"Condition").ToArray(), _actualConditionTypes);

            Assert.IsLessThan(1, notExisting.Length,
                    $"\nCondition{(1 < notExisting.Length ?"s":"")} missing are: \n{String.Join('\n', notExisting)}, \nthough supported by {nameof(ConditionFactory)}");
        }

        [TestMethod]
        public void ExistingConditionsAreAvailable()
        {
            string[] unavailableConditions = ExistingAreAvailable(_actualConditionTypes, typeof(ConditionTypeEnum), ConditionScratchLength);

            if (0 < unavailableConditions.Length) 
            { 
                Assert.Inconclusive($"\n⚠️ Conditions not available: \n{String.Join('\n', unavailableConditions)}. \nConsider adding them to {nameof(ConditionTypeEnum)}, if they are meant to be supported and available.");
            }
        }

        [TestMethod]
        public void ConditionModelImplementsRequiredInterfaces()
        {
            string[] missingInterfaces = ModelImplementsRequiredInterfaces(_actualConditionTypes, typeof(ConditionModel));
            
            Assert.IsLessThan(1, missingInterfaces.Length,
                $"\n{nameof(ConditionModel)} does not implement all required interfaces for: \n{String.Join('\n', missingInterfaces)}.");
        }

        [TestMethod]
        public void ConditionModelHasUselessInterfaces()
        {
            string[] extraInterfaces = ModelHasUselessInterfaces(typeof(ConditionModel), typeof(ConditionTypeEnum), ConditionScratchLength);

            foreach (string conditionInterface in extraInterfaces)
            {
                Console.WriteLine($"⚠️ Warning, unused interface{(0<extraInterfaces.Length?"s":"")} in {nameof(ConditionModel)}: \n{String.Join('\n', extraInterfaces)}.");
            }
        }


        
        private static string[] AvailableAreSupported(string[] availableTypes, string[] supportedTypes )
        {
            string[] notSupported = [];

            foreach (string available in availableTypes)
            {
                if (!supportedTypes.Contains(available)) 
                { notSupported = [..notSupported, available]; }
            }

            return notSupported;
        }

        private static string[] SupportedActuallyExists(string[] supportedTypes, string[] existingTypes)
        {
            string[] notExisting = [];

            foreach (string supported in supportedTypes)
            {
                if (!existingTypes.Contains(supported))
                { notExisting = [..notExisting, supported]; }
            }

            return notExisting;
        }

        private static string[] ExistingAreAvailable(string[] existingTypes, Type enumType, int scratchLength)
        {
            string[] notAvailable = [];
            
            foreach (string existing in existingTypes)
            {
                if (!Enum.TryParse(enumType, existing[..^scratchLength], false, out _))
                { notAvailable = [..notAvailable, existing]; }
            }

            return notAvailable;

        }

        private static string[] ModelImplementsRequiredInterfaces(string[] existingTypes, Type modelType)
        {
            string[] interfaces = modelType.GetInterfaces().Select(a => a.Name).ToArray();

            string[] missingInterfaces = [];

            foreach (string existing in existingTypes)
            {
                if ( !interfaces.Contains("I" + existing) )
                { missingInterfaces = [..missingInterfaces, existing]; }
            }

            return missingInterfaces;
        }

        private static string[] ModelHasUselessInterfaces(Type modelType, Type relatedEnumType, int scratchLength)
        {
            string[] interfaces = modelType.GetInterfaces().Select(a => a.Name).ToArray();

            string[] extraInterfaces = [];

            foreach (string interfaceName in interfaces)
            {
                if (!Enum.TryParse(relatedEnumType, interfaceName[1..^scratchLength], out _))
                {
                    extraInterfaces = [..extraInterfaces, interfaceName];
                }
            }

            return extraInterfaces;
        }
    }
}
