// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CharacterInfo
{
	public string Server;
	public long EntityId;
	public string Name;
	public DeletionFlag DeletionFlag;

	public bool IsPet { get { return (this.EntityId >= MabiId.Pets); } }
	public bool IsPartner { get { return (this.EntityId >= MabiId.Partners); } }
}
