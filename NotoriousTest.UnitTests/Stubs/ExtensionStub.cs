namespace NotoriousTest.UnitTests.Stubs
{
    public class ExtensionStub<T> : IInfrastructureExtension<T> where T : NotoriousTest.Core.Infrastructures.Infrastructure
    {
        public Action<T>? OnBeforeInitializeAction { get; set; }
        public Action<T>? OnAfterInitializeAction { get; set; }
        public Action<T>? OnBeforeResetAction { get; set; }
        public Action<T>? OnAfterResetAction { get; set; }
        public Action<T>? OnBeforeDestroyAction { get; set; }
        public Action<T>? OnAfterDestroyAction { get; set; }

        public Task OnBeforeInitialize(T infrastructure) { OnBeforeInitializeAction?.Invoke(infrastructure); return Task.CompletedTask; }
        public Task OnAfterInitialize(T infrastructure) { OnAfterInitializeAction?.Invoke(infrastructure); return Task.CompletedTask; }
        public Task OnBeforeReset(T infrastructure) { OnBeforeResetAction?.Invoke(infrastructure); return Task.CompletedTask; }
        public Task OnAfterReset(T infrastructure) { OnAfterResetAction?.Invoke(infrastructure); return Task.CompletedTask; }
        public Task OnBeforeDestroy(T infrastructure) { OnBeforeDestroyAction?.Invoke(infrastructure); return Task.CompletedTask; }
        public Task OnAfterDestroy(T infrastructure) { OnAfterDestroyAction?.Invoke(infrastructure); return Task.CompletedTask; }
    }
}
