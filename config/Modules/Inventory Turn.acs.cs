// Inventory Turn
// Written by Moss

Event::Attach(eventEnterStation, inv::turn);

function inv::turn()
{
	schedule("postAction(2048, IDACTION_TURNLEFT, 0.33);", 0.01);
	schedule("postAction(2048, IDACTION_TURNLEFT, -0);", 0.31);
}