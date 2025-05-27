namespace AzureMcp.Options.KeyVault.Key
{
    public class KeyListOptions : BaseKeyVaultOptions
    {
        public bool IncludeManagedKeys { get; set; } = false;
    }
}
