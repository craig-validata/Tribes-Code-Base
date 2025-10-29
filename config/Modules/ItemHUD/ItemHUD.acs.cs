$RepKit::Texture::Kit	 = "kitdot.png";

function ItemHUD::Init() {
	if ($ItemHUD::Loaded)
		return;
	$ItemHUD::Loaded = true;

	$ItemHUD::Awake = false;
	$ItemHUD::Grenades = 0;
	$ItemHUD::Kits = 0;
	
	HUD::New( "ItemHUD::Container", 200, 200, 130, 30, ItemHUD::Wake, ItemHUD::Sleep );
	newObject( "ItemHUD::Text", FearGuiFormattedText, 0, 0, 6, 12 );
	HUD::Add( "ItemHUD::Container", "ItemHUD::Text" );
	newObject( "ItemHUD::Beacons", FearGuiFormattedText, 35, 4, 20, 20 );
	HUD::Add( "ItemHUD::Container", "ItemHUD::Beacons" );
}

function ItemHUD::Wake() {
	$ItemHUD::Awake = true;
	ItemHUD::Update();
}

function ItemHUD::Sleep() {
	Schedule::Cancel("ItemHUD::Update();");
	$ItemHUD::Awake = false;
}

function ItemHUD::Update() {
	if ( !$ItemHUD::Awake )
		return;

	Schedule::Add("ItemHUD::Update();", 1);
	
	%text = "";
	%kits = getItemCount("Repair Kit");
	%Grenades = getItemCount("Grenade");
	%Beacons = getItemCount("Beacon");
	%WHOLELINE = "<F0>G: <F2>" ~%Grenades ~ "<F0> B: <F2>"~%Beacons  ;

//	dont bother updating if count hasnt changed
	 if ( ( %kits == $ItemHUD::Kits ) && ( %Grenades == $ItemHUD::Grenades )  && ( %Beacons == $ItemHUD::Beacons ))
		 return;

	$ItemHUD::Kits = %kits;
	$ItemHUD::Grenades = %Grenades;
	$ItemHUD::Beacons = %Beacons;
 
	%kits = ( %kits > 0 ) ? $RepKit::Texture::Kit : "blankdot.png";
	%text = "<B0,0:modules/itemhud/" @ %kits @ ">";
	
	Control::SetValue( "ItemHUD::Text", %text );
	Control::SetValue( "ItemHUD::Beacons", %WHOLELINE );
}

ItemHUD::Init();

Event::Attach(eventItemReceived, "Schedule::Add(\"ItemHUD::Update();\", 0);");
Event::Attach(eventItemDropped, "Schedule::Add(\"ItemHUD::Update();\", 0);");
Event::Attach(eventItemUsed, "Schedule::Add(\"ItemHUD::Update();\", 0);");
