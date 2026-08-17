using System;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class InteractableSpecialMemory: InteractableMemory
{

	public override void activate() {
		base.activate();
		//todo: cause extra action on memory, like pick up umbrella
	}

}
