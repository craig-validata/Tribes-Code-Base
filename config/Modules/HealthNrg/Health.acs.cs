function GHealth::Init() {
	if($GHealth:Loaded)
		return;
	$GHealth:Loaded = true;

// the stock tribes shaded background around the bar. default is false, no background
	%background = false;
	
	switch(%background){
		case "false":
			HUD::New( "GHealth::Container", 0, 0, 182, 15, GHealth::Wake, GHealth::Sleep );
			break;
		case "true":
			HUD::New::Shaded( "GHealth::Container", 0, 0, 182, 15, GHealth::Wake, GHealth::Sleep );
			break;
		default:
			HUD::New( "GHealth::Container", 0, 0, 182, 15, GHealth::Wake, GHealth::Sleep );
			break;		
	}

	newObject("GHealth::HF", FearGuiFormattedText, 6, 0, 170, 14);
	newObject("GHealth::HB", FearGuiFormattedText, 11, 3, 160, 8);
	
	HUD::Add("GHealth::Container","GHealth::HF");
	HUD::Add("GHealth::Container","GHealth::HB");

	Control::SetValue("GHealth::HF", "<B0,0:Modules/HealthNrg/frame.png>");
	Control::SetValue("GHealth::HB", "<B0,0:Modules/HealthNrg/healthbar.png>");
}

function GHealth::Wake() { GHealth::Update(); }
function GHealth::Sleep() { Schedule::Cancel("GHealth::Update();");}

function GHealth::Update() {
	Control::SetExtent("GHealth::HB", $health*1.6, 8 );
	Schedule::Add("GHealth::Update();",0.1);
}

GHealth::Init();

