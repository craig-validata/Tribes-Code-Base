// Thanks GreyHound
//
// 12 Favs for 1.40
// Ported by Giever and DaJ4ck3L

function GameBinds::SetMapNoClearBinds( %sae ) 
{
    $GameBinds::CurrentMap = %sae;
    $GameBinds::CurrentMapHandle = GameBinds::GetActionMap2( %sae );
    //ActionMapList::clearBinds( $GameBinds::CurrentMapHandle );
}

//typical hack to insert binds
function ChatDisplay::addBindsToMenu() after GameBinds::Init
{
	GameBinds::SetMapNoClearBinds( "actionMap.sae" );
	GameBinds::addBindCommand( "Buy/Select Loadout #6", "AutoBuy::SelectAndBuyLoadout(6);" );
    GameBinds::addBindCommand( "Buy/Select Loadout #7", "AutoBuy::SelectAndBuyLoadout(7);" );
    GameBinds::addBindCommand( "Buy/Select Loadout #8", "AutoBuy::SelectAndBuyLoadout(8);" );
    GameBinds::addBindCommand( "Buy/Select Loadout #9", "AutoBuy::SelectAndBuyLoadout(9);" );
    GameBinds::addBindCommand( "Buy/Select Loadout #10","AutoBuy::SelectAndBuyLoadout(10);" );
    GameBinds::addBindCommand( "Buy/Select Loadout #11","AutoBuy::SelectAndBuyLoadout(11);" );
    GameBinds::addBindCommand( "Buy/Select Loadout #12","AutoBuy::SelectAndBuyLoadout(12);" );
}


//------------------------------------
//
// Game/network functions
//
//------------------------------------
// Overwrite these functions
//

function CmdInventoryGui::onOpen() {
	$curFavorites = clamp( $curFavorites, 1, 12 ); //12 Favs -DaJ4ck3L
	CmdInventoryGui::favoritesSel( $curFavorites );
	Control::performClick( "FavButton" @ $curFavorites );

	Control::setActive( "Favorite::Name", $Station::Type != "" );
	Control::setFocus( "Favorite::Name", false ); // keeps grabbing focus
}

function CmdInventoryGui::favoritesSel(%favList)
{
   $curFavorites = %favList;
   CmdInventory::refreshFavorites( $curFavorites, $ServerFavoritesKey );
   %name = $pref::favorites[$curFavorites,$ServerFavoritesKey,"name"];
   if ( %name == "" )
   	   %name = "<Loadout Name>";
   Control::setText( "Favorite::Name", %name );
	for ( %i = 1; %i <= 12; %i++ ) //12 Favs -DaJ4ck3L
		Control::setValue( "FavButton" @ %i, false );
	Control::setValue( "FavButton" @ $curFavorites, true );
	Control::setActive( "Favorite::Name", $Station::Type != "" );
}

function CmdInventoryGui::buyFavorites( %fav ) {
   if ( %fav != "" )
      $curFavorites = %fav;
   if ( $curFavorites == -1 )
   	   return;

	//hilite the favs button
	if ( $curFavorites >= 1 && $curFavorites <= 12 ) //12 Favs -DaJ4ck3L
		CmdInventoryGui::favoritesSel( $curFavorites );

	%items = String::explode( $pref::favorites[$curFavorites, $ServerFavoritesKey], ",", "temp" );
	%cmd = "remoteEval(2048, buyFavorites";
	for ( %i = 0; %i < %items; %i++ )
		%cmd = sprintf( "%1, %2", %cmd, getItemType($temp[%i]) );
	evalf( "%1 );", %cmd );
}