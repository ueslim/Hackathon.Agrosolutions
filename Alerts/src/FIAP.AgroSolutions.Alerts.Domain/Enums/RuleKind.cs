public enum RuleKind
{
    ThresholdDuration = 1,        // condição precisa durar X tempo
    ThresholdInstantCooldown = 2,  // dispara na hora com cooldown
    WindowSumThreshold = 3,
    DualMetricDuration = 4
}