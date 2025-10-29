// Grooves NON-ScriptGL config color style changer & keybind
// updated 2013, cleaned and remoteEP
// removing this file will lock huds to the default Black style ?

$Colorchange::Color[1] = "Gray";
$Colorchange::Color[2] = "Black Milk";
$Colorchange::Color[3] = "Green";
$Colorchange::Color[4] = "BE Red";
$Colorchange::Color[5] = "DS Blue";
$Colorchange::currentstyle = 1;

// increment counter, loop at 5, set pref, remoteEP style name, apply the style
function Colorchange::styleup()	{
	$Colorchange::currentstyle += 1;
	if ($Colorchange::currentstyle > 5) $Colorchange::currentstyle = 1;
	$pref::vhud::Background::style = $Colorchange::Color[$Colorchange::currentstyle];
	remoteEP("<L5>HUD Style set to <f2>" @ $Colorchange::Color[$Colorchange::currentstyle] @ ".", 5, 1,1, 14, 300);
	Colorchange::applystyle();
	}

// apply the texture settings changes for the various styles
function Colorchange::applystyle()
{
	switch($pref::vhud::Background::style)
	{
		case "Black Milk":
			Colorchange::default(); 
			break;
		case "DS Blue":
			Control::SetValue("LeftBG::BG", "<B0,0:Modules/Backdrops/Blu_FlagHUD.png>");	
			Control::SetValue("RightBG::BG", "<B0,0:Modules/Backdrops/Blu_right.png>");	
			$RepKit::Texture::Kit	 = "blue_kit.png";
			break;
		case "Green":
			Control::SetValue("LeftBG::BG", "<B0,0:Modules/Backdrops/gglow_left.png>");	
			Control::SetValue("RightBG::BG", "<B0,0:Modules/Backdrops/gglow_right.png>");	
			$RepKit::Texture::Kit	 = "yerp.png";
			break;
		case "BE Red":
			Control::SetValue("LeftBG::BG", "<B0,0:Modules/Backdrops/rglow_FlagHUD.png>");	
			Control::SetValue("RightBG::BG", "<B0,0:Modules/Backdrops/rglow_right.png>");	
			$RepKit::Texture::Kit	 = "kitdot.png";
			break;
		case "Gray":
			Control::SetValue("LeftBG::BG", "<B0,0:Modules/Backdrops/gray_HUD.png>");	
			Control::SetValue("RightBG::BG", "<B0,0:Modules/Backdrops/gray_right.png>");	
			$RepKit::Texture::Kit	 = "gray_rkit.png";
			break;
		default:
			Colorchange::default();
			break;
	}
}

function Colorchange::default() {
	Control::SetValue("LeftBG::BG", "<B0,0:Modules/Backdrops/Black_Left.png>");	
	Control::SetValue("RightBG::BG", "<B0,0:Modules/Backdrops/Black_right.png>");	
	$RepKit::Texture::Kit	 = "kitdot.png";
	}

//hack to insert binds into the menu
function Colorchange::addBindsToMenu() after GameBinds::Init
{
	$GameBinds::CurrentMap = "actionMap.sae";
	$GameBinds::CurrentMapHandle = GameBinds::GetActionMap2( "actionMap.sae" );
	GameBinds::addBindCommand( "Change HUD Style", "Colorchange::styleup();", "" );
}

Event::attach(eventGuiOpen, Colorchange::applystyle);