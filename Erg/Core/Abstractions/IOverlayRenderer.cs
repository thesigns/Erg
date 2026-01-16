namespace Erg.Core.Abstractions;

public interface IOverlayRenderer
{
    void QueueHealthBar(int col, int row, float healthPercent);
    void ClearOverlays();
}
