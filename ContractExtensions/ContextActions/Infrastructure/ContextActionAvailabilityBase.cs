using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;

namespace ReSharper.ContractExtensions.ContextActions.Infrastructure
{
    /// <summary>
    /// Non-generic base class that represent wether specific context action is available or not.
    /// </summary>
    public abstract class ContextActionAvailabilityBase
    {
        protected bool _isAvailable;
        protected readonly ICSharpContextActionDataProvider _provider;

        protected ContextActionAvailabilityBase(ICSharpContextActionDataProvider provider)
        {
            _provider = provider;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || Provider != null,
                "For available context action provider should exist.");
        }

        public bool IsAvailable { get { return _isAvailable; } }


        public ICSharpContextActionDataProvider Provider { get { return _provider; } }
    }

    /// <summary>
    /// Generic base class that represent wether specific context action is available or not.
    /// </summary>
    /// <typeparam name="TConcreteAvailability"></typeparam>
    [ContractClass(typeof (ContextActionAvailabilityBaseContract<>))]
    public abstract class ContextActionAvailabilityBase<TConcreteAvailability> : ContextActionAvailabilityBase
        where TConcreteAvailability : ContextActionAvailabilityBase<TConcreteAvailability>, new()
    {
        protected ContextActionAvailabilityBase()
            : base(null)
        {}

        protected ContextActionAvailabilityBase(ICSharpContextActionDataProvider provider)
            : base(provider)
        {
            Contract.Requires(provider != null);
        }

        protected virtual void CheckAvailability()
        {}

        public static readonly TConcreteAvailability Unavailable = new TConcreteAvailability();

        /// <summary>
        /// Factory method that concrete availability from the <paramref name="provider"/>.
        /// </summary>
        /// <remarks>
        /// The main idea for this method is to create template method with "polymorphic" step called during
        /// object creation.
        /// </remarks>
        public static TConcreteAvailability CheckIsAvailable(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            // TODO: consider to change visibility for descendant constructors to private or protected!
            var ctor =
                typeof (TConcreteAvailability).GetConstructor(new Type[] {typeof (ICSharpContextActionDataProvider)});

            Contract.Assert(ctor != null, "Descendant should have a constructor that accepts ICSharpContextActionDataProvider");

            var result = (TConcreteAvailability)ctor.Invoke(new object[] { provider});
            result.CheckAvailability();
            return result;
        }
    }

    [ContractClassFor(typeof(ContextActionAvailabilityBase<>))]
    abstract class ContextActionAvailabilityBaseContract<T> : ContextActionAvailabilityBase<T> where T : ContextActionAvailabilityBase<T>, new()
    {}

}