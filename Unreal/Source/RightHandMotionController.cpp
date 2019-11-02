// Fill out your copyright notice in the Description page of Project Settings.

#include "RightHandMotionController.h"
#include "Public/MotionControllerComponent.h" 
#include "Components/SkeletalMeshComponent.h" 
#include "Components/SphereComponent.h" 
#include "SteamVRChaperoneComponent.h"

#include "UObject/ConstructorHelpers.h"

#include "Animation/AnimBlueprint.h" 

#include "Equipment/PlayerSword.h"
#include "Components/StaticMeshComponent.h" 
#include "Engine/StreamableManager.h"
#include "Item/Potion.h"					
#include "Kismet/GameplayStatics.h"							
#include "MyCharacter/MotionControllerCharacter.h"
#include "Item/PotionBag.h"
#include "GameInstance/VRGameInstance.h"

#include "Components/BoxComponent.h"
#include "Monster/Dog/Dog.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Components/CapsuleComponent.h"

#include "Kismet/KismetMathLibrary.h"

#include "Components/WidgetInteractionComponent.h"

#include "Monster/Dog/Dog.h"
#include "Monster/Dog/DogAIController.h"
#include "Object/Door/LockKey.h"		
#include "Object/Door/DoorLock.h"		
#include "Object/Door/LockedDoor.h"	

// 剣を持った右手のコードです。
// 攻撃以外にポーションを掴め、犬のモンスターに噛まれると、この腕に付けられるようになります。

// Sets default values
ARightHandMotionController::ARightHandMotionController()
{
	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	// ルートを生成、適用するコード
	MotionController = CreateDefaultSubobject<UMotionControllerComponent>(TEXT("MotionController"));
	SetRootComponent(MotionController);

	// 手形状のメッシュを生成、ルートにつけるコード
	HandMesh = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("HandMesh"));
	HandMesh->SetupAttachment(MotionController);

	// エディタ上でメッシュを探して適用
	static ConstructorHelpers::FObjectFinder<USkeletalMesh>SK_RightHand(TEXT("SkeletalMesh'/Game/Assets/CharacterEquipment/MyCharacter/Hand/Mesh/MannequinHand_Right.MannequinHand_Right'"));
	if (SK_RightHand.Succeeded()) 
	{
		HandMesh->SetSkeletalMesh(SK_RightHand.Object);
	}

	// 手形状のメッシュの位置、大きさ、属性を設定するコード
	HandMesh->SetRelativeLocation(FVector(-15.0f, 1.9f, 9.9f));
	HandMesh->SetRelativeRotation(FRotator(-45.0f, 0, 90.0f)); 
	HandMesh->bGenerateOverlapEvents = true;		
	HandMesh->SetCollisionProfileName(FName("NoCollision"));	

	//手回りの物体を感知するためのコリジョンを生成、手形状のメッシュに付けるコード
	OverlapSphere = CreateDefaultSubobject<USphereComponent>(TEXT("OverlapSphere"));
	OverlapSphere->SetupAttachment(HandMesh);

	// コリジョンの位置、大きさ、属性を設定するコード
	OverlapSphere->SetCollisionProfileName("OverlapAll");
	OverlapSphere->SetRelativeLocation(FVector(9.0f, 3.4f, -1.6f));
	OverlapSphere->SetSphereRadius(7.0f);
	OverlapSphere->bHiddenInGame = true;

	// ウィジェットと相互作用するコンポーネント生成及びコリジョンに付けるコード
	interaction = CreateDefaultSubobject<UWidgetInteractionComponent>(TEXT("Interaction"));
	interaction->SetupAttachment(OverlapSphere);

	// 相互作用のコンポーネントの設定コード
	interaction->TraceChannel = ECollisionChannel::ECC_Visibility;
	interaction->InteractionDistance = 100.0f;

	// 右手の相互作用領域を表示するコンポーネントであるSteamVRChaperoneコンポーネントを生成する。
	SteamVRChaperone = CreateDefaultSubobject<USteamVRChaperoneComponent>(TEXT("SteamVRChaperone"));

	// 鍵を生成する位置コンポーネントの生成及び手につけるコンポーネント
	KeySocket = CreateDefaultSubobject<USceneComponent>(TEXT("KeySocket"));
	KeySocket->SetupAttachment(HandMesh);

	// 鍵を生成する位置コンポーネントの位置及び角度
	KeySocket->SetRelativeLocation(FVector(10.0f, 0.0f, 0.5f));
	KeySocket->SetRelativeRotation(FRotator(88.0f, 180.0f, 180.0f));

	// ポーション位置コンポーネント生成および手メッシュに付着
	PotionPosition = CreateDefaultSubobject<USceneComponent>(TEXT("PotionPosition"));
	PotionPosition->SetupAttachment(HandMesh);

	// ポーション位置コンポーネントの位置及び角度設定
	PotionPosition->SetRelativeLocation(FVector(11.768803f, 0.847741f, -3.021931f));
	PotionPosition->SetRelativeRotation(FRotator(3.364257f, -86.961861f, 177.434341f));

	// 犬が嚙む位置のコンポーネント生成及び手のメッシュに付着
	AttachDogPosition = CreateDefaultSubobject<USceneComponent>(TEXT("AttachDogPosition"));
	AttachDogPosition->SetupAttachment(HandMesh);

	// 犬が嚙む位置のコンポーネントの位置及び角度設定
	AttachDogPosition->SetRelativeLocation(FVector(-10.0f, 71.845093f, -38.933586f));
	AttachDogPosition->SetRelativeRotation(FRotator(0.0f, -90.0f, 0.0f));

	// 剣の位置のコンポーネント生成及び手メッシュに付着
	SwordAttachScene = CreateDefaultSubobject<USceneComponent>(TEXT("WeaponScene"));
	SwordAttachScene->SetupAttachment(HandMesh);
						
	// 剣の位置のコンポーネントの位置及び角度設定
	SwordAttachScene->SetRelativeRotation(FRotator(0, 60.0f, -175.082443f)); 
	SwordAttachScene->SetRelativeLocation(FVector(3.238229f, 5.621831f, -3.814407f)); 

	// 他のチーム員が作成したコードです。
	//モーションコントローラのMotionSourceに入れるために列挙型を名前に型変換して
	FString HandName = GetEnumToString(Hand);
	MotionController->MotionSource = (FName(*HandName)); //入れる

	//手に使うアニメーションブループリントをエディターで見つけてABP_Handに入れる
	static ConstructorHelpers::FObjectFinder<UClass>ABP_Hand(TEXT("AnimBlueprint'/Game/Blueprints/MyCharacter/Hand/ABP_RightHand.ABP_RightHand_C'"));

	if (ABP_Hand.Succeeded()) //アニメーションブループリントを見つけたら
	{
		UClass* RightHandAnimBlueprint = ABP_Hand.Object;

		if (RightHandAnimBlueprint)
		{
			HandMesh->SetAnimationMode(EAnimationMode::AnimationBlueprint); //手のアニメーションモードをブループリント型に変えて
			HandMesh->SetAnimInstanceClass(RightHandAnimBlueprint); //上から探したアニメーションブループリントを入れる
		}
	}
	// ここまで
	
	// 初期値設定(剣を握っている状態)
	WantToGrip = true;
	AttachedActor = nullptr;
	HandState = E_HandState::Grab;
	HandFormState = EHandFormState::WeaponHandGrab;
	Hand = EControllerHand::Right;
	VisibleSwordFlag = true; 
	HandTouchActorFlag = true;
	bisRightGrab = false;
	bGrabPotion = false;	

	Tags.Add(FName(TEXT("RightHand"))); 
	Tags.Add(FName(TEXT("DisregardForLeftHand")));
	Tags.Add(FName(TEXT("DisregardForRightHand")));
}

// Called when the game starts or when spawned
void ARightHandMotionController::BeginPlay()
{
	Super::BeginPlay();

	// Owner設定
	HandOwner = Cast<AMotionControllerCharacter>(UGameplayStatics::GetPlayerCharacter(GetWorld(), 0));

	FActorSpawnParameters SpawnActorOption;		// アクターを生成する際に使われる構造変数
	SpawnActorOption.Owner = this;		// 生成する アクターの主人を現在クラスに決める.

	//	アクターを生成するときに衝突処理をどのようにするかを決めるオプション
	SpawnActorOption.SpawnCollisionHandlingOverride = ESpawnActorCollisionHandlingMethod::AlwaysSpawn;	  // アクターを生成する時、衝突と関係なく生成する

	FAttachmentTransformRules AttachRules(EAttachmentRule::SnapToTarget, EAttachmentRule::SnapToTarget, EAttachmentRule::KeepWorld, false);

	// 現在ワールドに剣を生成する
	Sword = GetWorld()->SpawnActor<APlayerSword>(Sword->StaticClass(), SwordAttachScene->GetComponentLocation(), SwordAttachScene->GetComponentRotation(), SpawnActorOption);

	if (Sword)
	{
		Sword->AttachToComponent(HandMesh, AttachRules, TEXT("CharacterSwordSocket"));
	}

	// オーバーラップによるバインディング関数
	OverlapSphere->OnComponentBeginOverlap.AddDynamic(this, &ARightHandMotionController::OnHandBeginOverlap);
	OverlapSphere->OnComponentEndOverlap.AddDynamic(this, &ARightHandMotionController::OnHandEndOverlap);
}

// Called every frame
void ARightHandMotionController::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	AActor* NearestMesh;	// 手回りにあるアクターを保存する変数

	// 摑んでいる状態、Grab状態、剣がある状態の場合
	if (AttachedActor != nullptr || WantToGrip == true || VisibleSwordFlag)
	{
		HandState = E_HandState::Grab;	// Grab
	}
	else
	{
		NearestMesh = GetActorNearHand();		// 手回りのアクター確認
		if (NearestMesh != nullptr)		// アクターが存在する時
		{
			HandState = E_HandState::CanGrab;		// 握れる状態に変える
		}
		else									// アクターが存在しない時
		{
			if (WantToGrip)			// アクターがないのにGrab状態なら
			{
				HandState = E_HandState::Grab;		// Grab状態に変える
			}
			else						// アクターもないしGrab状態でもなければ
			{
				HandState = E_HandState::Open;		 // オープン状態に変える
			}
		}
	}

	// 犬が腕を噛んでいる時、速度によってスタックを変化させる -> 一定スタック以上になると犬が落ちる
	if (AttachDog)
	{
		Prelinear = Currentlinear;

		HandCurrentPosistion = OverlapSphere->GetComponentLocation() - GetActorLocation();
		HandMoveDelta = HandCurrentPosistion - HandPreviousPosistion;
		HandMoveVelocity = HandMoveDelta / DeltaTime;
		HandPreviousPosistion = HandCurrentPosistion;

		Currentlinear = HandMoveVelocity.Size();

		if (Currentlinear > 45.0f) stack++;
		else
		{
			if (stack > 0) stack = 0;
		}
	}
}

// Grabボタンを押した時、呼び出される関数
void ARightHandMotionController::GrabActor()
{
	if (bGrabKey) return;
	AActor* NearestMesh;		// 近くにあるアクターを保存しておく変数

	// 握っている状態に変える
	WantToGrip = true;			
	bisRightGrab = true;

	if (HandTouchActorFlag)			// 手にアクターがぶつかった状態の場合
	{
		HandFormState = EHandFormState::PotionHandGrab;
		NearestMesh = GetActorNearHand(); 
		if (NearestMesh)			// 近くにアクターが存在
		{
			Sword->SetActorHiddenInGame(true);	// 剣を隠す
			// ドア
			if (NearestMesh->ActorHasTag("Door"))
			{
				AttachedActor = NearestMesh;
				// ドアを開けるものなら手を追うことはない。
			}

			// 他のチーム員が作成したコードです。
			// ポーション
			else if (NearestMesh->ActorHasTag("PotionBag"))
			{
				APotionBag* PotionBag = Cast<APotionBag>(NearestMesh);

				// ポーションかばんでGrabをする場合、残ったポーションがある時に手に生成する。
				if (PotionBag)
				{
					if (PotionBag->Potions.Num()>0)
					{
						// 剣を生成するときのように生成
						FActorSpawnParameters SpawnActorOption; 
						SpawnActorOption.Owner = this; 
						SpawnActorOption.SpawnCollisionHandlingOverride = ESpawnActorCollisionHandlingMethod::AlwaysSpawn;
						FAttachmentTransformRules AttachRules(EAttachmentRule::SnapToTarget, EAttachmentRule::SnapToTarget, EAttachmentRule::KeepWorld, false);
						Potion = PotionBag->PotionPop();

						// ポーションを生成して付着
						if (Potion)
						{
							AttachedActor = Potion;
							if (!Potion->Mesh->IsPendingKill())
							{
								Potion->Mesh->SetCollisionProfileName(TEXT("OverlapAll"));
								Potion->SetActorRelativeScale3D(FVector(1.1f, 1.1f, 1.1f));
								Potion->AttachToComponent(PotionPosition, AttachRules);
								HandGrabState();
								bGrabPotion = true;
							}							
						}

						// ゲームインスタンスで現在ポーションの本数を更新する
						UVRGameInstance* GI = Cast<UVRGameInstance>(UGameplayStatics::GetGameInstance(GetWorld()));

						if (GI)
						{
							GI->PotionCountUpdate(PotionBag->PotionCount);
						}
					}
					else
					{
						// 残ったポーションがない場合は作用をしない
						HandNomalState();
					}
				}
			}
			// ここまで
			else
			{
				// その他のものの作用
				AttachedActor = NearestMesh;
				FAttachmentTransformRules AttachRules(EAttachmentRule::KeepWorld, EAttachmentRule::KeepWorld, EAttachmentRule::KeepWorld, false);
				NearestMesh->GetRootComponent()->AttachToComponent(MotionController, AttachRules);
			}
		}
	}
}

// Grab解除
void ARightHandMotionController::ReleaseActor()
{
	if (bGrabKey) return;

	bisRightGrab = false;
	if (AttachedActor)
	{
		HandFormState = EHandFormState::WeaponHandGrab;
		WantToGrip = false;
		VisibleSwordFlag = false;	

		// つかんだものを取り外す
		if (AttachedActor->GetRootComponent()->GetAttachParent() == MotionController)
		{
			// ドアは除外
			if (!AttachedActor->ActorHasTag("Door"))
			{
				AttachedActor->DetachFromActor(FDetachmentTransformRules::KeepWorldTransform);
			}
		}
		// ポーションを取った時
		else if (AttachedActor->GetRootComponent()->GetAttachParent() == PotionPosition)
		{
			if (AttachedActor->ActorHasTag("Potion"))
			{
				APotion* AttachPotion = Cast<APotion>(AttachedActor);
				if (AttachPotion)
				{
					AActor* NearestMesh = GetActorNearHand();

					if (NearestMesh)
					{
						if (NearestMesh->ActorHasTag("PotionBag"))
						{
							// ポーションかばんの範囲内でGrab解除した場合は
							// ポーションを入れる。
							APotionBag* PotionBag = Cast<APotionBag>(NearestMesh);
							if (PotionBag)
							{
								PotionBag->PotionPush(AttachPotion);
								HandNomalState();
							}
							// 以外のところに落とすコード
							else
							{
								bGrabPotion = false;
								AttachPotion->BagInputFlag = true;
								AttachPotion->Mesh->SetCollisionProfileName("NoCollision");
								AttachPotion->Mesh->SetSimulatePhysics(true);
								AttachedActor->DetachFromActor(FDetachmentTransformRules::KeepWorldTransform);		
							}
						}
					}
					else
					{
						AttachPotion->BagInputFlag = true;
						AttachPotion->Mesh->SetSimulatePhysics(true);
						AttachedActor->DetachFromActor(FDetachmentTransformRules::KeepWorldTransform);
					}
				}
			}
		}
		AttachedActor = nullptr;	
	}
	else			// メニューのボタンから元の生態に戻る(メニューの範囲にいるときに剣のない手が開いた状態だった)
	{
		if (Sword->bHidden)		
			HandOpenState();		// オープン状態に変換
	}
}

// 近いアクター獲得
AActor * ARightHandMotionController::GetActorNearHand()
{
	TArray<AActor*> OverlappingActors;

	FVector GrabSphereLocation;
	FVector OverlappingActorLocation;
	FVector SubActorLocation;
	AActor* NearestOverlappingActor = nullptr;
	float NearestOverlap = 10000.0f;

	OverlapSphere->GetOverlappingActors(OverlappingActors, AActor::StaticClass());			// オーバーラップされたアクターを保存する
	GrabSphereLocation = OverlapSphere->GetComponentLocation();

	for (AActor* OverlappingActor : OverlappingActors)
	{
		if (OverlappingActor->ActorHasTag("DisregardForRightHand") || OverlappingActor->ActorHasTag("DisregardForLeftHand"))
		{
			continue;
		}
		else if (OverlappingActor->ActorHasTag("PotionBag"))
		{
			NearestOverlappingActor = OverlappingActor;
			break;
		}
		else if (OverlappingActor->ActorHasTag("Door"))
		{
			NearestOverlappingActor = OverlappingActor;
			break;
		}
		else
		{
			OverlappingActorLocation = OverlappingActor->GetActorLocation();
			SubActorLocation = OverlappingActorLocation - GrabSphereLocation;
			if (SubActorLocation.Size() < NearestOverlap)
			{
				NearestOverlappingActor = OverlappingActor;
				break;
			}
		}
	}
	return NearestOverlappingActor;
}

// 剣を握っている普通の状態
void ARightHandMotionController::HandNomalState()
{
	HandTouchActorFlag = false;
	WantToGrip = true;
	VisibleSwordFlag = true;
	Sword->SetActorHiddenInGame(false); 
	AttachedActor = nullptr;		
}

// 剣が見えずに手を伸ばしている状態
void ARightHandMotionController::HandOpenState()
{
	HandTouchActorFlag = true;
	WantToGrip = false;
	VisibleSwordFlag = false;
	Sword->SetActorHiddenInGame(true); 
}

// 剣が見えずに握っている状態
void ARightHandMotionController::HandGrabState()
{
	HandTouchActorFlag = false;
	WantToGrip = true;
	VisibleSwordFlag = false;
	Sword->SetActorHiddenInGame(true);
}

// オーバーラップイベント
void ARightHandMotionController::OnHandBeginOverlap(UPrimitiveComponent * OverlappedComponent, AActor * OtherActor, UPrimitiveComponent * OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult & SweepResult)
{
	if (OtherActor->ActorHasTag("LockedDoor"))
	{
		if (HandOwner->bHasKey)
			LockedDoor = Cast<ALockedDoor>(OtherActor);
	}


	if (OtherComp->ComponentHasTag("BodyLock"))
	{
		if (HandOwner->bHasKey)
		{
			LockedDoor->GetLockKey();
			HandNomalState();
			Key->Destroy();
			LockedDoor = NULL;
		}
		return;
	}

	if (bGrabKey) return;

	// 鍵を所持している状態で、錠の範囲内に入ると鍵が生成される。
	if (OtherComp->ComponentHasTag("LockArea"))
	{
		if (HandOwner->bHasKey)
		{
			bGrabKey = true;
			HandGrabState();
			FActorSpawnParameters SpawnActorOption; 
			SpawnActorOption.Owner = this; 
			SpawnActorOption.SpawnCollisionHandlingOverride = ESpawnActorCollisionHandlingMethod::AlwaysSpawn;
			FAttachmentTransformRules AttachRules(EAttachmentRule::SnapToTarget, EAttachmentRule::SnapToTarget, EAttachmentRule::KeepWorld, false);

			Key = GetWorld()->SpawnActor<ALockKey>(Key->StaticClass(), KeySocket->GetComponentLocation(), KeySocket->GetComponentRotation(), SpawnActorOption);

			if (Key)
			{
				Key->AttachToComponent(KeySocket, AttachRules);
			}
		}
		return;
	}

	// 種類: ポーションかばん、頭、ドア、その他アクター
	if (OtherComp->ComponentHasTag("DisregardForLeftHand") || OtherComp->ComponentHasTag("DisregardForRightHand"))
	{
		if (OtherComp->ComponentHasTag("Head"))
		{			
			HandNomalState();			
		}
		return;
	}
	if (AttachedActor)
	{
		return;
	}
	// メニューの空間範囲内に入ると、剣が消えたオープン状態にする
	if (OtherComp->ComponentHasTag("GrabRange"))
	{
		HandOpenState();
		return;
	}

	if (OtherActor->ActorHasTag("PotionBag"))		
	{
		HandOpenState();
		return;
	}
	else if (OtherActor->ActorHasTag("Door"))	
	{
		HandOpenState();
		return;
	}
	else if (OtherActor->ActorHasTag("DisregardForRightHand"))
	{
		return;
	}
	else 	
	{
		HandOpenState();
		return;
	}
}

// オーバーラップ解除イベント
void ARightHandMotionController::OnHandEndOverlap(UPrimitiveComponent * OverlappedComponent, AActor * OtherActor, UPrimitiveComponent * OtherComp, int32 OtherBodyIndex)
{
	if (OtherActor->ActorHasTag("LockedDoor"))
		LockedDoor = NULL;

	// 錠範囲外に行けば鍵除去
	if (OtherComp->ComponentHasTag("LockArea"))
	{
		if (Key)
		{
			bGrabKey = false;
			Key->Destroy();
		}
		HandNomalState();
	}

	if (OtherComp->ComponentHasTag("DisregardForLeftHand") || OtherComp->ComponentHasTag("DisregardForRightHand"))
		return;

	// メニューの空間範囲を出ると剣を摑んだ普通の状態にする
	if (OtherComp->ComponentHasTag("GrabRange"))
	{
		HandNomalState();
		return;
	}

	if (OtherActor->ActorHasTag("DisregardForRightHand"))
		return;

	if (HandState != E_HandState::Grab)
		HandNomalState();

	return;
}

// 他のチーム員が作成したコードです。
FString ARightHandMotionController::GetEnumToString(EControllerHand Value)
{
	UEnum* enumPtr = FindObject<UEnum>(ANY_PACKAGE, TEXT("EControllerHand"), true);
	if (!enumPtr)
	{
		return FString("Invalid");
	}
	return enumPtr->GetEnumName((int32)Value);
}
// ここまで