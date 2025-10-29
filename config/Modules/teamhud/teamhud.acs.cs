// Groove Team size HUD
function TeamHUD::Init()
{
	if ($TeamHUD::Loaded)
		return;
	$TeamHUD::Loaded = true;
	
	HUD::New ("TeamHUD::Container", 400, 0, 75, 18, TeamHUD::Wake, TeamHUD::Sleep);
	newObject("TeamHUD::BG", FearGuiFormattedText, 0, 0, 50, 17);
	newObject("TeamHUD::Friends", FearGuiFormattedText, 1, 0, 25, 17);
	newObject("TeamHUD::Foes", FearGuiFormattedText, 26, 0, 25, 17);
	newObject("TeamHUD::Obs", FearGuiFormattedText, 52, 0, 25, 17);
	HUD::Add("TeamHUD::Container","TeamHUD::BG");
	HUD::Add("TeamHUD::Container", "TeamHUD::Friends");
	HUD::Add("TeamHUD::Container", "TeamHUD::Foes");
	HUD::Add("TeamHUD::Container", "TeamHUD::Obs");
}

function TeamHUD::Update()
{
	%o = Team::Size( -1 );
	%f = Team::Size( Team::Friendly() );
	%e = Team::Size( Team::Enemy() );

	Control::SetValue("TeamHUD::BG", "<B0,0:Modules/teamhud/teams.png>");	
	Control::setValue("TeamHUD::Friends", "<f2> " @ %f); 
	Control::setValue("TeamHUD::Foes", "<f2> " @ %e); 
	Control::setValue("TeamHUD::Obs", "<f2> " @ %o); 
}

function TeamHUD::Wake() {}
function TeamHUD::Sleep() {}

Event::Attach(eventClientsUpdated, TeamHUD::Update);

TeamHUD::Init();