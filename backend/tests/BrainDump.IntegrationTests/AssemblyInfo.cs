// Auth tests mutate process-wide environment variables to drive Program.cs
// configuration. Running them in parallel would let those env vars leak across
// tests; force serial execution for the whole assembly to avoid the leakage.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
