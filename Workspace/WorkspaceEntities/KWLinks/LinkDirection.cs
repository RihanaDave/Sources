namespace GPAS.Workspace.Entities.KWLinks
{
    /// <summary>
    /// انواع جهت های ممکن برای یک رابطه و یا وابستگی بین دو شی
    /// </summary>
    public enum LinkDirection
    {
        SourceToTarget = 1,
        TargetToSource = 2,
        Bidirectional = 3
    }
}
