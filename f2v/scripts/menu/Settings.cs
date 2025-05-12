using Godot;
using System;
using System.Linq;

public partial class Settings : TabContainer
{
    public override void _Ready()
    {
    }

    private void _on_focus_entered()
    {
        _on_tab_changed(CurrentTab);
    }

    private void _on_tab_changed(int tabIndex)
    {
        // Récupérer le nœud de l'onglet actif
        var currentTab = GetChild(tabIndex);

        // Trouver le premier bouton dans l'onglet
        var firstButton = FindFirstButton(currentTab);

        // Si un bouton est trouvé, lui donner le focus
        if (firstButton != null)
        {
            firstButton.GrabFocus();
        }

        GD.Print("Tab changed to: " + currentTab.Name);
        GD.Print("First button: " + (firstButton != null ? firstButton.Name : "None"));
    }

    private Control FindFirstButton(Node node)
    {
        // Si le nœud est un bouton, le retourner
        if (node is Button button)
        {
            return button;
        }

        // Parcourir les enfants récursivement
        foreach (Node child in node.GetChildren())
        {
            var foundButton = FindFirstButton(child);
            if (foundButton != null)
                return foundButton;
        }

        return null;
    }
}
