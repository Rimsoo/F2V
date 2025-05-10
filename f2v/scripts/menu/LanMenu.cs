using Godot;
using System.Linq;
using System;

public partial class LanMenu : GridContainer
{
    [Export] PackedScene GameScene;

    private BoxContainer _playerList;
    private TextEdit _nameEdit;
    private TextEdit _ipEdit;
    private TextEdit _portEdit;
    private TextEdit _maxClientsEdit;
    private Button _hostButton;
    private Button _joinButton;
    private Button _startButton;

    private ENetMultiplayerPeer peer;

    public override void _Ready()
    {
        _ipEdit = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Ip")) as TextEdit;
        _portEdit = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Port")) as TextEdit;
        _maxClientsEdit = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("MaxClients")) as TextEdit;
        _hostButton = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Host")) as Button;
        _joinButton = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Join")) as Button;
        _nameEdit = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Name")) as TextEdit;
        _startButton = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("Start")) as Button;

        _playerList = GetTree().GetNodesInGroup("menu_buttons").First(mb => mb.Name.Equals("PlayerList")) as BoxContainer;

        _hostButton.Pressed += _on_host_pressed;
        _joinButton.Pressed += _on_join_pressed;
        _startButton.Pressed += _on_start_pressed;

        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;

        RefreshPlayerList();
    }

    private void ConnectionFailed()
    {
        GD.Print("Connection failed");
    }

    private void ConnectedToServer()
    {
        GD.Print("Connected to server");
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Peer disconnected: " + id);
        Rpc("SendRemovePlayerInformation", id);
    }

    private void PeerConnected(long id)
    {
        GD.Print("Peer connected: " + id);
        Rpc("SendAddPlayerInformation", _nameEdit.Text, Multiplayer.GetUniqueId());
    }

    private void _on_host_pressed()
    {
        GD.Print("Host button pressed");
        if (_hostButton.Text == "Stop")
        {
            GD.Print("Stopping server");
            foreach (var player in GameManager.Players)
            {
                RpcId(1, "SendRemovePlayerInformation", player.Id);
            }
            Multiplayer.MultiplayerPeer.Close();
            Multiplayer.MultiplayerPeer = null;
            _hostButton.Text = "Host";
            return;
        }
        Multiplayer.MultiplayerPeer = null;
        peer = new ENetMultiplayerPeer();
        GD.Print("Creating server with IP: " + _ipEdit.GetText() + " and port: " + _portEdit.GetText() + " and max clients: " + _maxClientsEdit.GetText());
        var err = peer.CreateServer(_portEdit.GetText().ToInt(), _maxClientsEdit.GetText().ToInt());
        if (err != Error.Ok)
        {
            GD.Print("Error creating server: " + err);
            return;
        }
        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        _hostButton.Text = "Stop";

        SendAddPlayerInformation(_nameEdit.Text, 1);
        _startButton.Disabled = false;
    }

    private void _on_join_pressed()
    {
        if (_joinButton.Text == "Disconnect")
        {
            GD.Print("Disconnecting from server");
            Multiplayer.MultiplayerPeer.Close();
            GameManager.Players.Clear();
            RefreshPlayerList();
            _joinButton.Text = "Join";
            return;
        }
        peer = new ENetMultiplayerPeer();
        var err = peer.CreateClient(_ipEdit.GetText(), _portEdit.GetText().ToInt());
        if (err != Error.Ok /*|| peer.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected*/)
        {
            GD.Print("Error creating client: " + err);
            return;
        }
        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        _joinButton.Text = "Disconnect";
    }

    public void _on_start_pressed()
    {
        Rpc("StartGame");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SendAddPlayerInformation(string name, long id)
    {
        if (GameManager.Players.Find(p => p.Id == id) == null)
        {
            GD.Print("Adding player: " + name + " with id: " + id);
            GameManager.Players.Add(new PlayerInfo()
            {
                Name = name,
                Id = id
            });
        }

        if (Multiplayer.IsServer())
        {
            foreach (var player in GameManager.Players)
            {
                Rpc("SendAddPlayerInformation", player.Name, player.Id);
            }
        }
        RefreshPlayerList();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SendRemovePlayerInformation(long id)
    {
        GameManager.Players.Remove(GameManager.Players.First<PlayerInfo>(i => i.Id == id));
        var players = GetTree().GetNodesInGroup("Players");

        foreach (var item in players)
        {
            if (item.Name == id.ToString())
            {
                item.QueueFree();
            }
        }

        if (Multiplayer.IsServer())
        {
            foreach (var player in GameManager.Players)
            {
                Rpc("SendRemovePlayerInformation", id);
            }
        }
        RefreshPlayerList();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RefreshPlayerList()
    {
        // Supprime tous les enfants actuels
        foreach (Node child in _playerList.GetChildren())
        {
            child.QueueFree(); // Ou _playerList.RemoveChild(child);
        }

        // Ajoute le titre
        _playerList.AddChild(new Label() { Text = "Players" });

        // Ajoute les joueurs
        foreach (var player in GameManager.Players)
        {
            _playerList.AddChild(new Label() { Text = $"{player.Name} ({player.Id})" });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void StartGame()
    {
        CleanupExistingGame();
        var game = GameScene.Instantiate<Game>();
        GetTree().Root.AddChild(game);
        GetParent().GetParent<Menu>().Visible = false;
    }

    public bool IsGameLoaded()
    {
        return GetTree().Root.GetChildren().OfType<Game>().Count() != 0;
    }

    private void CleanupExistingGame()
    {
        if (!IsGameLoaded())
            return;
        // Récupérer la référence de la game existante
        Game existingGame = GetTree().Root.GetChildren().OfType<Game>().FirstOrDefault();

        // Supprimer proprement l'ancienne instance
        if (existingGame != null)
        {
            existingGame.QueueFree(); // Suppression propre
            GetTree().Root.RemoveChild(existingGame); // Retrait explicite
        }

        // Nettoyer les références
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
