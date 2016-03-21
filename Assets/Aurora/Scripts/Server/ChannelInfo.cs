// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;

public class ChannelInfo
{
	public string Name;
	public ChannelState State;
	public ChannelEvent Events;
	public int Stress;

	public bool CanJoin { get { return State != ChannelState.Maintenance && State < ChannelState.Bursting; } }
}
