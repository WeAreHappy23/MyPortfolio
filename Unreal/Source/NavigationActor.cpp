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

// 道案内のアクター
// キャラクターが特定の地点に移動すると、このアクターはAIが提供する移動機能を使用して、次の位置に移動することになります。

// Sets default values
ANavigationActor::ANavigationActor()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	//  ルートを生成、適用するコード
	Scene = CreateDefaultSubobject<USceneComponent>(TEXT("Scene"));
	Scene->SetupAttachment(RootComponent);

	Navigate = CreateDefaultSubobject<UParticleSystemComponent>(TEXT("Navigate"));
	Navigate->SetupAttachment(Scene);

	// エディタ上でパーティクルを探して適用
	static ConstructorHelpers::FObjectFinder<UParticleSystem> P_Navigate(TEXT("ParticleSystem'/Game/Assets/Effect/Navigation/PS_GPP_Firefly.PS_GPP_Firefly'"));
	if (P_Navigate.Succeeded())
	{
		StartNavigate = P_Navigate.Object;
	}
	static ConstructorHelpers::FObjectFinder<UParticleSystem> P_EndNavigate(TEXT("ParticleSystem'/Game/Assets/Effect/Navigation/PS_GPP_CannonPurple_Explosion_2.PS_GPP_CannonPurple_Explosion_2'"));
	if (P_EndNavigate.Succeeded())
	{
		EndNavigate= P_EndNavigate.Object;
	}

	// エディタ上でAIファイルを探して適用
	static ConstructorHelpers::FObjectFinder<UBehaviorTree>Monster_BehaviorTree(TEXT("BehaviorTree'/Game/Blueprints/Object/Navigation/AI/BT_Navigation.BT_Navigation'"));
	if (Monster_BehaviorTree.Succeeded())
	{
		BehaviorTree = Monster_BehaviorTree.Object;
	}

	AIControllerClass = ANavigationAIController::StaticClass();

	// 現在ターゲット/目標ターゲット
	CurrentPoint = -1;
	TargetPoint = -1;

	// ターゲット
	Target = NULL;

	// タグ設定
	Tags.Add(FName("Navigation"));
}

// Called when the game starts or when spawned
void ANavigationActor::BeginPlay()
{
	Super::BeginPlay();
	
	// 道案内用のパーティクルに設定
	Navigate = UGameplayStatics::SpawnEmitterAttached(StartNavigate, RootComponent, NAME_None, GetActorLocation(), GetActorRotation(),
		EAttachLocation::KeepWorldPosition, false);

	if (Targets.Num() -1>= TargetPoint)
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

	// リアルタイムでAIに関する情報を適用
	if (AI)
	{
		AI->BBComponent->SetValueAsInt("CurrentPoint", CurrentPoint);
		AI->BBComponent->SetValueAsInt("TargetPoint", TargetPoint);
		AI->BBComponent->SetValueAsObject("Target", Target);
		AI->BBComponent->SetValueAsBool("bIsSame", CurrentPoint==TargetPoint);
		AI->BBComponent->SetValueAsBool("bIsMax", TargetPoint ==Targets.Num());
	}
}

// 次のターゲットに移動
void ANavigationActor::NavigationEvent()
{
	CurrentPoint++;
	TargetPoint++;

	// 最後の位置への移動でない時
	if (Targets.Num() > TargetPoint)
	{
		// 次のターゲットに移動した時、デリゲートであらかじめバインディングしてイベントを呼び出せるようにする。
		// あらかじめ次のターゲットのOnOverlapがNavigationEventを再び呼び出せるようにデリゲートバインディング
		if (Targets[TargetPoint])
		{
			Target = Targets[TargetPoint];
			if (Register)
				Register->Collision->bGenerateOverlapEvents = false;
			Register = Cast<ANavigationPoint>(Targets[TargetPoint]);
			Register->NaviEvent.BindUObject(this, &ANavigationActor::NavigationEvent);
		}
	}

	// 移動後最後の位置の時にこのアクターのパーティクルを変更
	if (CurrentPoint == Targets.Num() - 1)
	{
		Navigate->DeactivateSystem();
		TargetPoint = Targets.Num();
		if (EndNavigate)
			Navigate = UGameplayStatics::SpawnEmitterAttached(EndNavigate, RootComponent,
				NAME_None, GetActorLocation(), GetActorRotation(), EAttachLocation::KeepWorldPosition, false);
	}
}



