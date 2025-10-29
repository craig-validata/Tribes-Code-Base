//Old JumpJet Code
// Evita (1.40 mod by lemzorz)
//
// Automatically jumps before you jet.
//
//
//editActionMap("playMap.sae");
//bindCommand(mouse0, make, button1, TO, "JumpJet(1);");
//bindCommand(mouse0, break, button1, TO, "JumpJet(0);");
//bindCommand(keyboard0, make, "space", TO, "$skiOn = true; postaction( 2048, IDACTION_MOVEUP, 1);");
//bindCommand(keyboard0, break, "space", TO, "$skiOn = false; postaction( 2048, IDACTION_MOVEUP, 0);");
//
//function JumpJet(%val) 
//{
// if(%val) 
// {
//		if(getMountedItem(0) == -1)
//			 postAction(2048, IDACTION_JET, 1);
//		else 
//   {
//			 postAction(2048, IDACTION_MOVEUP, 1);
//			 postAction(2048, IDACTION_JET, 1);
//	}
// }
// else
// {
//	  if(!$skiOn) 
//	  { 
//		  postAction(2048, IDACTION_MOVEUP, 0); 
//	  } 
//	  postAction(2048, IDACTION_JET, 0); 
// }
//}

//Plasmatic's JumpJet and Ski
function PlasJet::addBindsToMenu() after GameBinds::Init
{
	GameBinds::SetMapNoClearBinds( "playMap.sae" );
	GameBinds::addBindCommand( "Plasmatic's JumpJet", "Plas::Jet();", "Plas::EndJet();" );
}
//editActionMap("playMap.sae");
//BindCommand(mouse0, make, button1, TO, "Plas::Jet();");
//bindAction(mouse0, break, button1, TO, IDACTION_JET, 0.000000);
function Plas::Jet()
{
	if(getMountedItem(0) != -1)	//check weapon slot -no jump jet with no weapon
		postAction(2048, IDACTION_MOVEUP, 1);
	postAction(2048,IDACTION_JET, 1.000000);	
}

function Plas::EndJet()
{
	postAction(2048,IDACTION_JET, 0);
}

//Bind for Ski
function PlasSki::addBindsToMenu() after GameBinds::Init
{
	GameBinds::SetMapNoClearBinds( "playMap.sae" );
	GameBinds::addBindCommand( "Plasmatic's Ski", "$Plas::skiing = true;Plas::Ski(true);", "$Plas::skiing = false;" );
}
//bindCommand(keyboard0, make, "space", TO, "$Plas::skiing = true;Plas::Ski(true);");
//bindCommand(keyboard0, break, "space", TO, "$Plas::skiing = false;");
function Plas::Ski()
{
	if($Plas::skiing)
	{
		postAction(2048, IDACTION_MOVEUP, 1);
		schedule("Plas::Ski();",0.05);	//jump jump jump	
	}
}