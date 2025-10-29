function Backdrops::Init() {
	if($Backdrops:Loaded)
		return;
	$Backdrops:Loaded = true;
	
	HUD::New( "LeftBG::Container", 0, 0, 404, 66, BG::NoSleep, BG::NoSleep );
 	newObject("LeftBG::BG", FearGuiFormattedText, 0, 0, 406, 66);
	HUD::Add("LeftBG::Container","LeftBG::BG");
	Control::SetValue("LeftBG::BG", "<B0,0:Modules/Backdrops/gglow_left.png>");	
	
	HUD::New( "RightBG::Container", 0, 0, 404, 66, BG::NoSleep, BG::NoSleep );
 	newObject("RightBG::BG", FearGuiFormattedText, 0, 0, 406, 66);
	HUD::Add("RightBG::Container","RightBG::BG");
	Control::SetValue("RightBG::BG", "<B0,0:Modules/Backdrops/gglow_right.png>");	
}
function BG::NoSleep() { }
Backdrops::Init();