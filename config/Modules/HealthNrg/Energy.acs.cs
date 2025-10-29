function GEnergy::Init() {
	if($GEnergy:Loaded)
		return;
	$GEnergy:Loaded = true;

	HUD::New( "GEnergy::Container", 0, 0, 182, 16, GEnergy::Wake, GEnergy::Sleep );
	
	newObject("GEnergy::EF", FearGuiFormattedText, 6, 0, 170, 18);
	newObject("GEnergy::EB", FearGuiFormattedText, 11, 3, 160, 8);
	HUD::Add("GEnergy::Container","GEnergy::EF");
	HUD::Add("GEnergy::Container","GEnergy::EB");
	Control::SetValue("GEnergy::EF", "<B0,0:Modules/HealthNrg/frame.png>");
	Control::SetValue("GEnergy::EB", "<B0,0:Modules/HealthNrg/energybar.png>");
}

function GEnergy::Wake() { GEnergy::Update(); }
function GEnergy::Sleep() { Schedule::Cancel("GEnergy::Update();"); }

function GEnergy::Update() {
	Control::SetExtent("GEnergy::EB", $energy*1.6, 8 );
	Schedule::Add("GEnergy::Update();",0.1);
}

GEnergy::Init();