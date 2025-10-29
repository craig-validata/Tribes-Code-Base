function Groove::DiscoBoogie()
{
	if (nameToID(PlayerSetupGui) == -1)
		return;

	switch($FGSkin::DiscoBoogie)
	{
	case "True":
		FGSkin::cycleArmor(IDCTG_PLAYER_TS);
		FGSkin::cycleArmor(IDCTG_PLAYER_TS);
		FGSkin::cycleArmor(IDCTG_PLAYER_TS);
		schedule("Groove::DiscoBoogie();", 3);
		break;
	case "False":
		Schedule::Cancel( "Groove::DiscoBoogie();" );
		FGSkin::set(IDCTG_PLAYER_TS, 0, $PCFG::Gender[$PCFG::CurrentPlayer]);
		FGSkin::cycleArmor(IDCTG_PLAYER_TS);
		FGSkin::cycleArmor(IDCTG_PLAYER_TS);
		FGSkin::cycleArmor(IDCTG_PLAYER_TS);
		break;
	default:
		echoc(2,"Discoboogie Default");
		FGSkin::cycleArmor(IDCTG_PLAYER_TS);
		FGSkin::cycleArmor(IDCTG_PLAYER_TS);
		FGSkin::cycleArmor(IDCTG_PLAYER_TS);
		schedule("Groove::DiscoBoogie();", 3);
		break;
	}
}