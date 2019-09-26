// Fill out your copyright notice in the Description page of Project Settings.

#include "NavigationActor.h"
#include "Object/NavigationActor/NavigationPoint.h"
#include "Particles/ParticleSystemComponent.h"
#include "UObject/ConstructorHelpers.h"
#include "BehaviorTree/BehaviorTree.h"
#include "Object/NavigationActor/NavigationAIController.h"
#include "Particles/ParticleSystem.h"
#include "ParticleDefinitions.h"
#include "BehaviorTree/BlackboardComponent.h"
#include "BehaviorTree/BehaviorTreeComponent.h"
#include "Kismet/GameplayStatics.h"
#include "Components/BoxComponent.h"

// ナビゲーションアクターです。
// プレーヤーがポイントに移動すれば,このナビゲーションアクターは次のターゲットに移動します。
// 'CurrentPoint'と'TargetPoint'が異なるときにビヘイビアツリーから次のターゲットに移動します。

// Sets default values
ANavigationActor::ANavigationActor()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	Scene = CreateDefaultSubobject<USceneComponent>(TEXT("Scene"));
	Scene->SetupAttachment(RootComponent);

	Navigate = CreateDefaultSubobject<UParticleSystemComponent>(TEXT("Navigate"));
	Navigate->SetupAttachment(Scene);

	// 始まりと終わりパーティクル
	static ConstructorHelpers::FObjectFinder<UParticleSystem> P_Navigate(TEXT("ParticleSystem'/Game/Assets/Effect/Navigation/PS_GPP_Firefly.PS_GPP_Firefly'"));
	if (P_Navigate.Succeeded())
	{
		StartNavigate = P_Navigate.Object;
	}
	static ConstructorHelpers::FObjectFinder<UParticleSystem> P_EndNavigate(TEXT("ParticleSystem'/Game/Assets/Effect/Navigation/PS_GPP_Butterfly.PS_GPP_Butterfly'"));
	if (P_EndNavigate.Succeeded())
	{
		EndNavigate= P_EndNavigate.Object;
	}

	// ビヘイビアツリーを探します。
	static ConstructorHelpers::FObjectFinder<UBehaviorTree>Monster_BehaviorTree(TEXT("BehaviorTree'/Game/Blueprints/Object/Navigation/AI/BT_Navigation.BT_Navigation'"));
	if (Monster_BehaviorTree.Succeeded())
	{
		BehaviorTree = Monster_BehaviorTree.Object;
	}

	AIControllerClass = ANavigationAIController::StaticClass();

	// 現在ターゲットと目標ターゲット
	CurrentPoint = -1;
	TargetPoint = -1;

	// ターゲット
	Target = NULL;

	Tags.Add(FName("Navigation"));
}

// Called when the game starts or when spawned
void ANavigationActor::BeginPlay()
{
	Super::BeginPlay();
	
	// SetTemplateは初めてしか使用できなくてSpawnEmitterを使用します。
	Navigate = UGameplayStatics::SpawnEmitterAttached(StartNavigate, RootComponent, NAME_None, GetActorLocation(), GetActorRotation(),
		EAttachLocation::KeepWorldPosition, false);

	// 移動ターゲットがあればデレゲートイベントに登録します。
	if (Targets.Num() -1 >= TargetPoint)
	{
		TargetPoint++;
		Target = Targets[TargetPoint];
		Register = Cast<ANavigationPoint>(Targets[TargetPoint]);
		Register->NaviEvent.BindUObject(this, &ANavigationActor::NavigationEvent);
	}
}

// Called every frame
void ANavigationActor::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
	
	AI = Cast<ANavigationAIController>(GetController());

	if (AI)
	{
		AI->BBComponent->SetValueAsInt("CurrentPoint", CurrentPoint);
		AI->BBComponent->SetValueAsInt("TargetPoint", TargetPoint);
		AI->BBComponent->SetValueAsObject("Target", Target);
		AI->BBComponent->SetValueAsBool("bIsSame", CurrentPoint==TargetPoint);
		AI->BBComponent->SetValueAsBool("bIsMax", TargetPoint ==Targets.Num());
	}
}

// ナビゲーションポイントでイベントを呼びすれば実行されます。
void ANavigationActor::NavigationEvent()
{
	CurrentPoint++;
	TargetPoint++;

	// ターゲットの再設定 / 次のターゲットにイベントを登録します。
	if (Targets.Num() > TargetPoint)
	{
		if (Targets[TargetPoint])
		{
			Target = Targets[TargetPoint];
			if (Register)
				Register->Collision->bGenerateOverlapEvents = false;
			Register = Cast<ANavigationPoint>(Targets[TargetPoint]);
			Register->NaviEvent.BindUObject(this, &ANavigationActor::NavigationEvent);
		}
	}

	// 最後のターゲットに移動すると消えるエフェクトとともにビヘイビアツリーから数秒後に消えることにします。
	if (CurrentPoint == Targets.Num() - 1)
	{
		Navigate->DeactivateSystem();

		if (EndNavigate)
			Navigate = UGameplayStatics::SpawnEmitterAttached(EndNavigate, RootComponent,
				NAME_None, GetActorLocation(), GetActorRotation(), EAttachLocation::KeepWorldPosition, false);
	}
}



